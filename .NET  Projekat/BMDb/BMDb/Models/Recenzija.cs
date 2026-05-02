namespace BMDb.Models
{
    public class Recenzija
    {
        public int Id { get; set; }
        public int Ocjena { get; set; }
        public string Komentar { get; set; }
        public DateTime DatumObjave { get; set; }
        public int OsobaId { get; set; }
        public int EntertainmentId { get; set; }

        public Recenzija() { }
    }
}
