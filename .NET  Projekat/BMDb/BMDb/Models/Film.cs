namespace BMDb.Models
{
    public class Film : Entertainment
    {
        public int IDFilma { get; set; }
        public int BoxOffice { get; set; }

        public Film() : base() { }
    }
}
