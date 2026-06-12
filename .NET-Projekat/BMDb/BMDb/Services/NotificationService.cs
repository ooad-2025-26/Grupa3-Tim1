using BMDb.Data;
using BMDb.Models;
using Microsoft.EntityFrameworkCore;

namespace BMDb.Services
{
    public interface IContentCreationObserver
    {
        Task OnContentCreatedAsync(Entertainment entertainment);
    }

    public interface INotificationService : IContentCreationObserver
    {
    }

    // Observer pattern: Film/Serija creation is the event source and this service reacts by creating notifications.
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnContentCreatedAsync(Entertainment entertainment)
        {
            var imaKorisnikaZaObavijest = await _context.Users.AnyAsync(x => x.NotifikacijeUkljucene);
            if (!imaKorisnikaZaObavijest)
            {
                return;
            }

            _context.Notifikacija.Add(new Notifikacija
            {
                Tekst = $"Novi naslov je dodan: {entertainment.Naziv}",
                DatumObjave = DateTime.UtcNow,
                Slika = entertainment.PosterLink ?? string.Empty
            });

            await _context.SaveChangesAsync();
        }
    }
}
