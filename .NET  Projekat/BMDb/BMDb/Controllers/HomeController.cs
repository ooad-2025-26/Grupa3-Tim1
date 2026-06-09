using System.Diagnostics;
using System.Security.Claims;
using BMDb.Data;
using BMDb.Models;
using BMDb.Services;
using BMDb.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BMDb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IRecommendationService _recommendationService;
        private readonly ITrailerServis _trailerServis;
        private readonly IUserKeyService _userKeyService;
        private readonly UserManager<Osoba> _userManager;
        private readonly IMediaImageService _mediaImageService;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            IRecommendationService recommendationService,
            ITrailerServis trailerServis,
            IUserKeyService userKeyService,
            UserManager<Osoba> userManager,
            IMediaImageService mediaImageService)
        {
            _logger = logger;
            _context = context;
            _recommendationService = recommendationService;
            _trailerServis = trailerServis;
            _userKeyService = userKeyService;
            _userManager = userManager;
            _mediaImageService = mediaImageService;
        }

        public IActionResult Index()
        {
            return View(); // Ovo otvara Views/Home/Index.cshtml
        }

        public async Task<IActionResult> Glavna()
        {
            var randomFilmovi = await _context.Film
                .AsNoTracking()
                .Select(x => (Entertainment)x)
                .ToListAsync();

            var randomSerije = await _context.Serija
                .AsNoTracking()
                .Select(x => (Entertainment)x)
                .ToListAsync();

            var randomSadrzaj = randomFilmovi
                .Concat(randomSerije)
                .OrderBy(_ => Random.Shared.Next())
                .Take(8)
                .ToList();

            var topFilmovi = await _context.Film
                .AsNoTracking()
                .OrderByDescending(x => x.ProsjecnaOcjena)
                .ThenByDescending(x => x.GodinaIzlaska)
                .Take(10)
                .ToListAsync();

            var topSerije = await _context.Serija
                .AsNoTracking()
                .OrderByDescending(x => x.ProsjecnaOcjena)
                .ThenByDescending(x => x.GodinaIzlaska)
                .Take(10)
                .ToListAsync();

            IReadOnlyList<Entertainment> preporuke = [];
            if (User.Identity?.IsAuthenticated == true &&
                (User.IsInRole("Korisnik") || User.IsInRole("VerifikovaniRecenzent")) &&
                !User.IsInRole("Admin") &&
                !User.IsInRole("Moderator"))
            {
                var osobaId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var gledaoSamOsobaId = _userKeyService.GetCurrentUserKey(User);
                preporuke = await _recommendationService.PersonalizovanePreporukeAsync(osobaId, gledaoSamOsobaId, 10);
            }

            var trenutnaGodina = DateTime.UtcNow.Year;
            var buduciFilmovi = await _context.Film
                .AsNoTracking()
                .Where(x => x.GodinaIzlaska > trenutnaGodina && x.YoutubeLink != null && x.YoutubeLink != string.Empty)
                .OrderBy(x => x.GodinaIzlaska)
                .ThenByDescending(x => x.ProsjecnaOcjena)
                .Take(10)
                .ToListAsync();

            var comingSoon = buduciFilmovi
                .Select(x => new { Film = x, TrailerEmbedUrl = _trailerServis.PokreniTrailer(x.YoutubeLink) })
                .Where(x => !string.IsNullOrWhiteSpace(x.TrailerEmbedUrl))
                .Take(5)
                .ToList();

            var sviIds = randomSadrzaj.Select(x => x.Id)
                .Concat(topFilmovi.Select(x => x.Id))
                .Concat(topSerije.Select(x => x.Id))
                .Concat(preporuke.Select(x => x.Id))
                .Concat(comingSoon.Select(x => x.Film.Id))
                .Distinct()
                .ToList();

            var zanrovi = await UcitajZanroveAsync(sviIds);

            var model = new HomeGlavnaViewModel
            {
                RandomMediaItems = randomSadrzaj.Select(x => MapMediaItem(x, zanrovi)).ToList(),
                TopRatedFilms = topFilmovi.Select(x => MapMediaItem(x, zanrovi)).ToList(),
                TopRatedSeries = topSerije.Select(x => MapMediaItem(x, zanrovi)).ToList(),
                RecommendedItems = preporuke.Select(x => MapMediaItem(x, zanrovi)).ToList(),
                ShowRecommendations = preporuke.Count > 0,
                ComingSoonFilms = comingSoon.Select(x => MapMediaItem(x.Film, zanrovi, x.TrailerEmbedUrl)).ToList()
            };

            return View(model);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard() { return View(); }

        [Authorize(Roles = "Admin")]
        public IActionResult Finansije() { return View(); }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> AdminLista()
        {
            var filmovi = await _context.Film
                .AsNoTracking()
                .OrderByDescending(x => x.GodinaIzlaska)
                .Select(x => (Entertainment)x)
                .ToListAsync();

            var serije = await _context.Serija
                .AsNoTracking()
                .OrderByDescending(x => x.GodinaIzlaska)
                .Select(x => (Entertainment)x)
                .ToListAsync();

            var items = filmovi.Concat(serije)
                .OrderByDescending(x => x.GodinaIzlaska)
                .ThenBy(x => x.Naziv)
                .ToList();

            var zanrovi = await UcitajZanroveAsync(items.Select(x => x.Id).ToList());
            return View(items.Select(x => MapMediaItem(x, zanrovi)).ToList());
        }

        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> Korisnici()
        {
            var users = await _userManager.Users
                .OrderBy(x => x.Ime)
                .ThenBy(x => x.Prezime)
                .ThenBy(x => x.Email)
                .ToListAsync();

            var model = new List<AdminUserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new AdminUserViewModel
                {
                    Id = user.Id,
                    FullName = string.Join(" ", new[] { user.Ime, user.Prezime }.Where(x => !string.IsNullOrWhiteSpace(x))),
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList()
                });
            }

            return View("~/Views/Osoba/Index.cshtml", model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            if (User.IsInRole("Moderator") && roles.Any(x => x == "Admin" || x == "Moderator"))
            {
                return Forbid();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                TempData["AdminUserError"] = string.Join(" ", result.Errors.Select(x => x.Description));
            }

            return RedirectToAction(nameof(Korisnici));
        }

        private async Task<Dictionary<int, IReadOnlyList<string>>> UcitajZanroveAsync(IReadOnlyList<int> entertainmentIds)
        {
            if (entertainmentIds.Count == 0)
            {
                return [];
            }

            var zanrovi = await (
                    from ez in _context.EntertainmentZanr.AsNoTracking()
                    join z in _context.Zanr.AsNoTracking() on ez.ZanrId equals z.Id
                    where entertainmentIds.Contains(ez.EntertainmentId)
                    select new { ez.EntertainmentId, z.Naziv }
                )
                .ToListAsync();

            return zanrovi
                .GroupBy(x => x.EntertainmentId)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<string>)g.Select(x => x.Naziv).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList());
        }

        private HomeMediaItemViewModel MapMediaItem(
            Entertainment entertainment,
            IReadOnlyDictionary<int, IReadOnlyList<string>> zanrovi,
            string trailerEmbedUrl = "")
        {
            return new HomeMediaItemViewModel
            {
                Id = entertainment.Id,
                Title = entertainment.Naziv ?? string.Empty,
                ControllerName = entertainment is Serija ? "Serija" : "Film",
                PosterUrl = _mediaImageService.ResolvePosterUrl(entertainment.PosterLink),
                Rating = entertainment.ProsjecnaOcjena,
                Year = entertainment.GodinaIzlaska > 0 ? entertainment.GodinaIzlaska : null,
                TrailerEmbedUrl = trailerEmbedUrl,
                Genres = zanrovi.TryGetValue(entertainment.Id, out var itemZanrovi) ? itemZanrovi : []
            };
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
