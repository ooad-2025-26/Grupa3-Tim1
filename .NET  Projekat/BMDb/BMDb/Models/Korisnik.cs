namespace BMDb.Models
{
    public class Korisnik : Osoba
    {
        public List<int> ListaGledao { get; set; } = new();
        public List<int> ListaGledatCe { get; set; } = new();
    }
}
