namespace BMDb.ViewModels
{
    public class ContentSearchFilter
    {
        public string? Search { get; set; }
        public int? ZanrId { get; set; }
        public int? Godina { get; set; }
        public double? MinimalnaOcjena { get; set; }
        public string? Sort { get; set; }
    }
}
