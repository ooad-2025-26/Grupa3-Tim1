namespace BMDb.ViewModels
{
    public class EntertainmentListItemViewModel
    {
        public int Id { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string Tip { get; set; } = string.Empty;
        public int GodinaIzlaska { get; set; }
        public double Ocjena { get; set; }
        public string? PosterLink { get; set; }
        public string ControllerName { get; set; } = string.Empty;
    }
}
