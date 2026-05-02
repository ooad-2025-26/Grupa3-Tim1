using System.ComponentModel.DataAnnotations;

namespace BMDb.Models
{
    public class Sezona
    {
        [Key]
        public int IdSezone { get; set; }
        public int IdSerije { get; set; }
        public int RedniBrojSezone { get; set; }
        public int BrojEpizoda { get; set; }
        public int DatumPremijere   { get; set; }
        public int PosterSezone { get; set; }

        public Sezona() { } 
    }
}
