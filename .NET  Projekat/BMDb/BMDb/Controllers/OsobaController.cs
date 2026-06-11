using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BMDb.Data;
using BMDb.Models;
using BMDb.ViewModels;
using Microsoft.EntityFrameworkCore;
using BMDb.Services;

namespace BMDb.Controllers
{
    [Authorize]
    public class OsobaController : Controller
    {
        private readonly UserManager<Osoba> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IMediaImageService _mediaImageService;

        public OsobaController(
            UserManager<Osoba> userManager,
            ApplicationDbContext context,
            IMediaImageService mediaImageService)
        {
            _userManager = userManager;
            _context = context;
            _mediaImageService = mediaImageService;
        }

        public async Task<IActionResult> Details()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            return View(await BuildProfileDetailsViewModelAsync(user));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ProfileDetailsViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            if (!ModelState.IsValid)
            {
                var invalidModel = await BuildProfileDetailsViewModelAsync(user);
                invalidModel.Ime = model.Ime;
                invalidModel.Prezime = model.Prezime;
                invalidModel.Nadimak = model.Nadimak;
                invalidModel.PhoneNumber = model.PhoneNumber;
                invalidModel.NotifikacijeUkljucene = model.NotifikacijeUkljucene;
                invalidModel.AvatarPath = model.AvatarPath;
                invalidModel.AvatarUrl = ResolveAvatarUrl(model.AvatarPath);
                invalidModel.SelectedZanrIds = model.SelectedZanrIds;
                return View(invalidModel);
            }

            user.Ime = model.Ime?.Trim() ?? string.Empty;
            user.Prezime = model.Prezime?.Trim() ?? string.Empty;
            user.Nadimak = model.Nadimak?.Trim() ?? string.Empty;
            user.NotifikacijeUkljucene = model.NotifikacijeUkljucene;
            user.Avatar = model.AvatarPath?.Trim() ?? string.Empty;

            if (!string.Equals(user.PhoneNumber, model.PhoneNumber, StringComparison.OrdinalIgnoreCase))
            {
                var phoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber?.Trim());
                if (!phoneResult.Succeeded)
                {
                    AddIdentityErrors(phoneResult);
                    return View(await BuildProfileDetailsViewModelAsync(user));
                }
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                AddIdentityErrors(updateResult);
                return View(await BuildProfileDetailsViewModelAsync(user));
            }

            await ReplacePreferredGenresAsync(user.Id, model.SelectedZanrIds);
            await _context.SaveChangesAsync();

            TempData["StatusPoruka"] = "Profil je uspjesno azuriran.";
            return RedirectToAction(nameof(Details));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAvatar(string? avatarPath)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Korisnik nije prijavljen." });
            }

            var trimmedAvatarPath = avatarPath?.Trim() ?? string.Empty;
            var validationError = ValidateAvatarPath(trimmedAvatarPath);
            if (validationError != null)
            {
                return BadRequest(new { success = false, message = validationError });
            }

            user.Avatar = trimmedAvatarPath;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BadRequest(new
                {
                    success = false,
                    message = updateResult.Errors.FirstOrDefault()?.Description ?? "Avatar nije sacuvan. Pokusajte ponovo."
                });
            }

            return Json(new
            {
                success = true,
                avatarUrl = ResolveAvatarUrl(user.Avatar)
            });
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Index()
        {
            return RedirectToAction("Korisnici", "Home");
        }

        private async Task<ProfileDetailsViewModel> BuildProfileDetailsViewModelAsync(Osoba user)
        {
            var odabraniZanrovi = await (
                from oz in _context.OsobaZanr.AsNoTracking()
                join z in _context.Zanr.AsNoTracking() on oz.ZanrId equals z.Id
                where oz.OsobaId == user.Id
                select new { z.Id, z.Naziv }
            )
            .Where(x => !string.IsNullOrWhiteSpace(x.Naziv))
            .Distinct()
            .OrderBy(x => x.Naziv)
            .ToListAsync();

            var sviZanrovi = await _context.Zanr
                .AsNoTracking()
                .Where(x => !string.IsNullOrWhiteSpace(x.Naziv))
                .OrderBy(x => x.Naziv)
                .ToListAsync();

            return new ProfileDetailsViewModel
            {
                User = user,
                Ime = user.Ime,
                Prezime = user.Prezime,
                Nadimak = user.Nadimak,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                NotifikacijeUkljucene = user.NotifikacijeUkljucene,
                AvatarPath = user.Avatar,
                AvatarUrl = ResolveAvatarUrl(user.Avatar),
                PreferiraniZanrovi = odabraniZanrovi.Select(x => x.Naziv).ToList(),
                SviZanrovi = sviZanrovi,
                SelectedZanrIds = odabraniZanrovi.Select(x => x.Id).Distinct().ToArray()
            };
        }

        private string ResolveAvatarUrl(string? avatarPath)
        {
            return string.IsNullOrWhiteSpace(avatarPath)
                ? "~/images/uploads/obicna.png"
                : _mediaImageService.ResolvePosterUrl(avatarPath);
        }

        private static string? ValidateAvatarPath(string avatarPath)
        {
            if (string.IsNullOrWhiteSpace(avatarPath))
            {
                return "Unesite URL ili putanju avatar slike.";
            }

            if (avatarPath.Length > 500)
            {
                return "Putanja avatar slike ne smije biti duza od 500 znakova.";
            }

            if (avatarPath.Contains("..") || avatarPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return "Putanja avatar slike nije validna.";
            }

            if (Uri.TryCreate(avatarPath, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps
                    ? null
                    : "Avatar URL mora poceti sa http:// ili https://.";
            }

            if (avatarPath.StartsWith("/", StringComparison.Ordinal) ||
                avatarPath.StartsWith("~/", StringComparison.Ordinal) ||
                !Path.IsPathRooted(avatarPath))
            {
                return null;
            }

            return "Putanja avatar slike nije validna.";
        }

        private async Task ReplacePreferredGenresAsync(string userId, int[] selectedZanrIds)
        {
            var existing = await _context.OsobaZanr
                .Where(x => x.OsobaId == userId)
                .ToListAsync();

            _context.OsobaZanr.RemoveRange(existing);

            var validZanrIds = await _context.Zanr
                .Where(x => selectedZanrIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToListAsync();

            foreach (var zanrId in validZanrIds.Distinct())
            {
                _context.OsobaZanr.Add(new OsobaZanr
                {
                    OsobaId = userId,
                    ZanrId = zanrId
                });
            }
        }

        private void AddIdentityErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
    }
}
