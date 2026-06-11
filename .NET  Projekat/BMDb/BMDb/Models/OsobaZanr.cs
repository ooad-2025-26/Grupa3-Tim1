namespace BMDb.Models
{
    public class OsobaZanr
    {
        public int Id { get; set; }
        public string OsobaId { get; set; }
        public int ZanrId { get; set; }
        public Osoba? Osoba { get; set; }
        public Zanr? Zanr { get; set; }
    }
}
