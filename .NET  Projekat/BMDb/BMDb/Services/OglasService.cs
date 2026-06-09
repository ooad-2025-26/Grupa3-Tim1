using BMDb.Data;
using BMDb.Models;
using Microsoft.EntityFrameworkCore;

namespace BMDb.Services
{
    public interface IOglasService
    {
        Task<Oglas?> DohvatiAktivniOglasAsync();
    }

    public class OglasService : IOglasService
    {
        private readonly ApplicationDbContext _context;

        public OglasService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Oglas?> DohvatiAktivniOglasAsync()
        {
            var aktivni = await _context.Oglas.Where(x => x.Aktivan).ToListAsync();
            if (aktivni.Count == 0)
            {
                return null;
            }

            return aktivni[Random.Shared.Next(aktivni.Count)];
        }
    }

    // Decorator pattern: Oglas remains data-only, while decorators add display behavior.
    public abstract class OglasDecorator
    {
        protected OglasDecorator(Oglas oglas)
        {
            Oglas = oglas;
        }

        protected Oglas Oglas { get; }
        public virtual string PrikaziOglas() => $"<a href=\"{Oglas.Link}\"><img src=\"{Oglas.Slika}\" alt=\"Oglas\" /></a>";
    }

    public class PremiumOglasDecorator : OglasDecorator
    {
        public PremiumOglasDecorator(Oglas oglas) : base(oglas)
        {
        }

        public override string PrikaziOglas()
        {
            return $"<div class=\"premium-oglas\">{base.PrikaziOglas()}</div>";
        }
    }
}
