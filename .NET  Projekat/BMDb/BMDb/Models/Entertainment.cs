namespace BMDb.Models
{
    public abstract class Entertainment
    {
        public int Id { get; set; }
        public string Naziv { get; set; }
        public string Opis { get; set; }
        public double ProsjecnaOcjena { get; set; }
        public string Reditelj { get; set; }
        public int GodinaIzlaska { get; set; }
        public string YoutubeLink { get; set; }
        public int Trajanje { get; set; }
        public string PosterLink { get; set; }

        public Entertainment() { }
    }
}
