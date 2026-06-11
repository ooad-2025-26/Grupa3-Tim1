using BMDb.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace BMDb.ViewModels
{
    public class ProfileMediaItemViewModel
    {
        public int EntertainmentId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string Opis { get; set; } = string.Empty;
        public double ProsjecnaOcjena { get; set; }
        public string Reditelj { get; set; } = string.Empty;
        public int GodinaIzlaska { get; set; }
        public int Trajanje { get; set; }
        public string PosterLink { get; set; } = string.Empty;
        public string ControllerName { get; set; } = "Film";
    }

    public class ProfileReviewViewModel
    {
        public int Id { get; set; }
        public int EntertainmentId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public int GodinaIzlaska { get; set; }
        public string PosterLink { get; set; } = string.Empty;
        public string ControllerName { get; set; } = "Film";
        public int Ocjena { get; set; }
        public string Komentar { get; set; } = string.Empty;
        public DateTime DatumObjave { get; set; }
    }

    public class ProfileDetailsViewModel
    {
        [ValidateNever]
        public Osoba User { get; set; } = new();

        [ValidateNever]
        public IReadOnlyList<string> PreferiraniZanrovi { get; set; } = Array.Empty<string>();

        [ValidateNever]
        public IReadOnlyList<Zanr> SviZanrovi { get; set; } = Array.Empty<Zanr>();

        public int[] SelectedZanrIds { get; set; } = Array.Empty<int>();

        [StringLength(100)]
        public string? Ime { get; set; }

        [StringLength(100)]
        public string? Prezime { get; set; }

        [StringLength(100)]
        public string? Nadimak { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public bool NotifikacijeUkljucene { get; set; }

        [StringLength(500)]
        public string? AvatarPath { get; set; }

        public string AvatarUrl { get; set; } = string.Empty;
    }
}
