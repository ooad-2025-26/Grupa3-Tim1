using System.ComponentModel.DataAnnotations;

namespace BMDb.Models
{
    public class Recenzija
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ocjena je obavezna.")]
        [Range(1, 10, ErrorMessage = "Ocjena mora biti između 1 i 10.")]
        public int Ocjena { get; set; }

        [StringLength(2000, ErrorMessage = "Komentar ne može biti duži od 2000 karaktera.")]
        public string Komentar { get; set; } = string.Empty;

        public DateTime DatumObjave { get; set; }
        public int OsobaId { get; set; }
        public int EntertainmentId { get; set; }

        public Recenzija() { }
    }
}
