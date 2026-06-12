namespace BMDb.Models
{
    public class Zanr
    {
        public int Id { get; set; }
        public string Naziv { get; set; }
        public ICollection<EntertainmentZanr> EntertainmentZanrovi { get; set; } = new List<EntertainmentZanr>();
        public ICollection<OsobaZanr> OsobaZanrovi { get; set; } = new List<OsobaZanr>();

        public Zanr() { }
    }
}
