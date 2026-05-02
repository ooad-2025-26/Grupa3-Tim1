namespace BMDb.Models
{
    public class Serija : Entertainment
    {
        public int IDSerije { get; set; }
        public int BrojSezona { get; set; }
        public int BrojEpizoda { get; set; }
        public bool ZavrsenoEmitovanje { get; set; }

        public Serija() : base() { }
    }
}
