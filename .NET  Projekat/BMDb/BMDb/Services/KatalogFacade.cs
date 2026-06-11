using BMDb.Data;
using BMDb.Models;
using BMDb.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BMDb.Services
{
    // Facade pattern: details pages ask one class for related media details instead of coordinating many DbSets.
    public class KatalogFacade
    {
        private readonly ApplicationDbContext _context;
        private readonly ITrailerServis _trailerServis;
        private readonly IWatchlistService _watchlistService;

        public KatalogFacade(ApplicationDbContext context, ITrailerServis trailerServis, IWatchlistService watchlistService)
        {
            _context = context;
            _trailerServis = trailerServis;
            _watchlistService = watchlistService;
        }

        public async Task<MediaDetailsViewModel?> DohvatiSveDetaljeAsync(int id, int osobaId)
        {
            var entertainment = await _context.Entertainment.FirstOrDefaultAsync(x => x.Id == id);
            if (entertainment == null)
            {
                return null;
            }

            var uloge = await _context.Uloga.Where(x => x.EntertainmentId == id).ToListAsync();
            var glumacIds = uloge.Select(x => x.GlumacId).Distinct().ToList();
            var recenzije = await _context.Recenzija
                .Where(x => x.EntertainmentId == id)
                .OrderByDescending(x => x.DatumObjave)
                .ToListAsync();
            var reviewData = await GetReviewDataAsync();
            var jeVecRecenzirao = osobaId > 0 && recenzije.Any(x => x.OsobaId == osobaId);
            var zanrovi = (await _context.EntertainmentZanr
                .AsNoTracking()
                .Where(x => x.EntertainmentId == id)
                .Join(
                    _context.Zanr.AsNoTracking(),
                    ez => ez.ZanrId,
                    z => z.Id,
                    (ez, z) => new { z.Id, z.Naziv })
                .Where(x => !string.IsNullOrWhiteSpace(x.Naziv))
                .Distinct()
                .OrderBy(x => x.Naziv)
                .ToListAsync())
                .Select(x => new Zanr { Id = x.Id, Naziv = x.Naziv })
                .ToList();

            return new MediaDetailsViewModel
            {
                Entertainment = entertainment,
                TrailerEmbedUrl = _trailerServis.PokreniTrailer(entertainment.YoutubeLink),
                Recenzije = recenzije,
                RegularRecenzije = recenzije,
                VerifikovaneRecenzije = recenzije.Where(x => reviewData.VerifiedUserKeys.Contains(x.OsobaId)).ToList(),
                Recenzenti = reviewData.Authors,
                Sezone = await _context.Sezona.Where(x => x.IdSerije == id).OrderBy(x => x.RedniBrojSezone).ToListAsync(),
                Uloge = uloge.OrderBy(x => x.Id).ToList(),
                Glumci = await _context.Glumac.Where(x => glumacIds.Contains(x.Id)).OrderBy(x => x.Ime).ThenBy(x => x.Prezime).ToListAsync(),
                Zanrovi = zanrovi,
                Galerija = await _context.GalerijaSlika.Where(x => x.EntertainmentId == id).OrderBy(x => x.Id).ToListAsync(),
                JeGledao = osobaId > 0 && await _watchlistService.JeGledaoAsync(osobaId, id),
                JePlanirano = osobaId > 0 && await _watchlistService.JePlaniranoAsync(osobaId, id),
                JePrijavljen = osobaId > 0,
                JeVecRecenzirao = jeVecRecenzirao
            };
        }

        private async Task<(Dictionary<int, ReviewAuthorViewModel> Authors, HashSet<int> VerifiedUserKeys)> GetReviewDataAsync()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Select(x => new
                {
                    x.Id,
                    x.Nadimak,
                    x.UserName,
                    x.Email,
                    x.Avatar
                })
                .ToListAsync();

            var userKeys = users.ToDictionary(x => x.Id, x => CreateUserKey(x.Id));
            var authors = users.ToDictionary(
                x => userKeys[x.Id],
                x => new ReviewAuthorViewModel
                {
                    DisplayName = FirstNonEmpty(x.Nadimak, x.UserName, x.Email, "Korisnik"),
                    Avatar = string.IsNullOrWhiteSpace(x.Avatar) ? "/images/uploads/user-img.png" : x.Avatar
                });

            var roleRows = await (
                    from userRole in _context.UserRoles.AsNoTracking()
                    join role in _context.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                    where role.Name == "VerifikovaniRecenzent"
                    select new { userRole.UserId, role.Name }
                )
                .ToListAsync();

            var verifiedUserKeys = roleRows
                .Where(x => x.Name == "VerifikovaniRecenzent" && userKeys.ContainsKey(x.UserId))
                .Select(x => userKeys[x.UserId])
                .ToHashSet();

            return (authors, verifiedUserKeys);
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            return values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "Korisnik";
        }

        private static int CreateUserKey(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return 0;
            }

            var bytes = MD5.HashData(Encoding.UTF8.GetBytes(id));
            return Math.Abs(BitConverter.ToInt32(bytes, 0));
        }
    }
}
