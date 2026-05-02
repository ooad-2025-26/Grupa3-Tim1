namespace BMDb.Models
{
    public class Notifikacija
    {
        public int Id { get; set; }
        public string Tekst { get; set; }
        public DateTime DatumObjave { get; set; }
        public string Slika { get; set; }

        public Notifikacija() { }
    }

  
}
