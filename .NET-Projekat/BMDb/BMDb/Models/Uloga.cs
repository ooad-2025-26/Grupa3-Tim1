namespace BMDb.Models
{
    public class Uloga
    {
        public int Id { get; set; }
        public string ImeLika { get; set; }
        public int GlumacId { get; set; }
        public int EntertainmentId { get; set; }

        public Uloga() { }
    }
}
