namespace BMDb.Models
{
    public class Oglas
    {
        public int Id { get; set; }
        public string Slika { get; set; }
        public string Link { get; set; }
        public bool Aktivan { get; set; }
        public double Prihod { get; set; }
        public int brojanjeOglasa { get; set; }

        public Oglas() { }
    }
}
