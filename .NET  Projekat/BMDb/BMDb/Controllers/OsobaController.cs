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
        private readonly IWebHostEnvironment _environment;
        private readonly IMediaImageService _mediaImageService;


        public OsobaController(
            UserManager<Osoba> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment environment,
            IMediaImageService mediaImageService)
        {
            _userManager = userManager;
            _context = context;
            _environment = environment;
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

            if (model.AvatarUpload != null && !IsAllowedAvatarFile(model.AvatarUpload.FileName))
            {
                ModelState.AddModelError(nameof(model.AvatarUpload), "Dozvoljeni formati su .jpg, .jpeg, .png i .webp.");
            }

            if (!ModelState.IsValid)
            {
                var invalidModel = await BuildProfileDetailsViewModelAsync(user);
                invalidModel.Ime = model.Ime;
                invalidModel.Prezime = model.Prezime;
                invalidModel.Nadimak = model.Nadimak;
                invalidModel.Email = model.Email;
                invalidModel.PhoneNumber = model.PhoneNumber;
                invalidModel.NotifikacijeUkljucene = model.NotifikacijeUkljucene;
                return View(invalidModel);
            }

            user.Ime = model.Ime?.Trim() ?? string.Empty;
            user.Prezime = model.Prezime?.Trim() ?? string.Empty;
            user.Nadimak = model.Nadimak?.Trim() ?? string.Empty;
            user.NotifikacijeUkljucene = model.NotifikacijeUkljucene;

            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var emailResult = await _userManager.SetEmailAsync(user, model.Email?.Trim());
                if (!emailResult.Succeeded)
                {
                    AddIdentityErrors(emailResult);
                    return View(await BuildProfileDetailsViewModelAsync(user));
                }
            }

            if (!string.Equals(user.PhoneNumber, model.PhoneNumber, StringComparison.OrdinalIgnoreCase))
            {
                var phoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber?.Trim());
                if (!phoneResult.Succeeded)
                {
                    AddIdentityErrors(phoneResult);
                    return View(await BuildProfileDetailsViewModelAsync(user));
                }
            }

            if (model.AvatarUpload != null && model.AvatarUpload.Length > 0)
            {
                user.Avatar = await SaveAvatarAsync(user.Id, model.AvatarUpload);
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                AddIdentityErrors(updateResult);
                return View(await BuildProfileDetailsViewModelAsync(user));
            }

            TempData["StatusPoruka"] = "Profil je uspješno ažuriran.";
            return RedirectToAction(nameof(Details));
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult Index()
        {
            return RedirectToAction("Korisnici", "Home");
        }

        private async Task<ProfileDetailsViewModel> BuildProfileDetailsViewModelAsync(Osoba user)
        {
            var zanrovi = await (
                from oz in _context.OsobaZanr.AsNoTracking()
                join z in _context.Zanr.AsNoTracking() on oz.ZanrId equals z.Id
                where oz.OsobaId == user.Id
                select z.Naziv
            ).ToListAsync();

            return new ProfileDetailsViewModel
            {
                User = user,
                Ime = user.Ime,
                Prezime = user.Prezime,
                Nadimak = user.Nadimak,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                NotifikacijeUkljucene = user.NotifikacijeUkljucene,
                AvatarUrl = string.IsNullOrWhiteSpace(user.Avatar)
                    ? "/images/uploads/user-img.png"
                    : _mediaImageService.ResolvePosterUrl(user.Avatar),
                PreferiraniZanrovi = zanrovi
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList()
            };
        }

        private static bool IsAllowedAvatarFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension is ".jpg" or ".jpeg" or ".png" or ".webp";
        }

        private async Task<string> SaveAvatarAsync(string userId, IFormFile avatar)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "images", "uploads");
            Directory.CreateDirectory(uploadsPath);

            var extension = Path.GetExtension(avatar.FileName).ToLowerInvariant();
            var fileName = $"avatar-{userId}-{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(uploadsPath, fileName);

            await using var stream = System.IO.File.Create(physicalPath);
            await avatar.CopyToAsync(stream);

            return $"/images/uploads/{fileName}";
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
