using System.Diagnostics;
using System.Security.Claims;
using BMDb.Data;
using BMDb.Models;
using BMDb.Services;
using BMDb.ViewModels;
using Microsoft.AspNetCore.Authorization;
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

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            IRecommendationService recommendationService,
            ITrailerServis trailerServis,
            IUserKeyService userKeyService)
        {
            _logger = logger;
            _context = context;
            _recommendationService = recommendationService;
            _trailerServis = trailerServis;
            _userKeyService = userKeyService;
        }

        public IActionResult Index()
        {
            return View(); // Ovo otvara Views/Home/Index.cshtml
        }

        public async Task<IActionResult> Glavna()
        {
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

            var osobaId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var gledaoSamOsobaId = User.Identity?.IsAuthenticated == true
                ? _userKeyService.GetCurrentUserKey(User)
                : 0;
            var preporuke = await _recommendationService.PersonalizovanePreporukeAsync(osobaId, gledaoSamOsobaId, 10);

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

            var sviIds = topFilmovi.Select(x => x.Id)
                .Concat(topSerije.Select(x => x.Id))
                .Concat(preporuke.Select(x => x.Id))
                .Concat(comingSoon.Select(x => x.Film.Id))
                .Distinct()
                .ToList();

            var zanrovi = await UcitajZanroveAsync(sviIds);

            var model = new HomeGlavnaViewModel
            {
                TopRatedFilms = topFilmovi.Select(x => MapMediaItem(x, zanrovi)).ToList(),
                TopRatedSeries = topSerije.Select(x => MapMediaItem(x, zanrovi)).ToList(),
                RecommendedItems = preporuke.Select(x => MapMediaItem(x, zanrovi)).ToList(),
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

        [Authorize(Roles = "Admin")]
        public IActionResult AdminLista() { return View(); }

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

        private static HomeMediaItemViewModel MapMediaItem(
            Entertainment entertainment,
            IReadOnlyDictionary<int, IReadOnlyList<string>> zanrovi,
            string trailerEmbedUrl = "")
        {
            return new HomeMediaItemViewModel
            {
                Id = entertainment.Id,
                Title = entertainment.Naziv ?? string.Empty,
                ControllerName = entertainment is Serija ? "Serija" : "Film",
                PosterUrl = string.IsNullOrWhiteSpace(entertainment.PosterLink)
                    ? "/images/uploads/mv-item1.jpg"
                    : entertainment.PosterLink,
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
