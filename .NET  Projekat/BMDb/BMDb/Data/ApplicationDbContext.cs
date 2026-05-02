using BMDb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BMDb.Data
{
    public class ApplicationDbContext : IdentityDbContext<Osoba, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Entertainment> Entertainment { get; set; }
        public DbSet<EntertainmentZanr> EntertainmentZanr { get; set; }
        public DbSet<Film> Film { get; set; }
        public DbSet<GalerijaSlika> GalerijaSlika { get; set; }
        public DbSet<GledaoSam> GledaoSam { get; set; }
        public DbSet<GledatCu> GledatCu { get; set; }
        public DbSet<Glumac> Glumac { get; set; }
        public DbSet<Notifikacija> Notifikacija { get; set; }
        public DbSet<Oglas> Oglas { get; set; }
        public DbSet<Recenzija> Recenzija { get; set; }
        public DbSet<Serija> Serija { get; set; }
        public DbSet<Sezona> Sezona { get; set; }
        public DbSet<Uloga> Uloga { get; set; }
        public DbSet<Zanr> Zanr { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Osoba>().ToTable("Osoba");
            modelBuilder.Entity<Entertainment>().ToTable("Entertainment");
            modelBuilder.Entity<EntertainmentZanr>().ToTable("EntertainmentZanr");
            modelBuilder.Entity<Film>().ToTable("Film");
            modelBuilder.Entity<GalerijaSlika>().ToTable("GalerijaSlika");
            modelBuilder.Entity<GledaoSam>().ToTable("GledaoSam");
            modelBuilder.Entity<GledatCu>().ToTable("GledatCu");
            modelBuilder.Entity<Glumac>().ToTable("Glumac");
            modelBuilder.Entity<Notifikacija>().ToTable("Notifikacija");
            modelBuilder.Entity<Oglas>().ToTable("Oglas");
            modelBuilder.Entity<Recenzija>().ToTable("Recenzija");
            modelBuilder.Entity<Serija>().ToTable("Serija");
            modelBuilder.Entity<Sezona>().ToTable("Sezona");
            modelBuilder.Entity<Uloga>().ToTable("Uloga");
            modelBuilder.Entity<Zanr>().ToTable("Zanr");
        }
    }
}