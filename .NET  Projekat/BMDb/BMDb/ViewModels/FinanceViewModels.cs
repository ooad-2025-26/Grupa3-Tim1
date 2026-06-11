namespace BMDb.ViewModels
{
    public class FinanceAdRowViewModel
    {
        public int Id { get; set; }
        public string Link { get; set; } = string.Empty;
        public bool Aktivan { get; set; }
        public double Prihod { get; set; }
        public int BrojanjeOglasa { get; set; }
        public double UkupniPrihod => BrojanjeOglasa * Prihod;
    }
}
