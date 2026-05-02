using BMDb.Models;
using Microsoft.AspNetCore.Identity;

public class Osoba : IdentityUser<int>
{
    public string Nadimak { get; set; }
    public string Ime { get; set; }
    public string Prezime { get; set; }
    public string Avatar { get; set; }
    public DateTime DatumRegistracije { get; set; }
    public bool NotifikacijeUkljucene { get; set; }
    public OsobaStatus StatusOsobe { get; set; }
    public int BrojRecenzija { get; set; }

    public Osoba() : base() { }
}