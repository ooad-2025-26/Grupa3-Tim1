using BMDb.ViewModels;

namespace BMDb.Services
{
    // Builder pattern: controller actions compose filter criteria without knowing construction details.
    public class ContentSearchFilterBuilder
    {
        private readonly ContentSearchFilter _filter = new();

        public ContentSearchFilterBuilder WithSearch(string? search)
        {
            _filter.Search = search;
            return this;
        }

        public ContentSearchFilterBuilder WithGenre(int? zanrId)
        {
            _filter.ZanrId = zanrId;
            return this;
        }

        public ContentSearchFilterBuilder WithYear(int? godina)
        {
            _filter.Godina = godina;
            return this;
        }

        public ContentSearchFilterBuilder WithRating(double? minimalnaOcjena)
        {
            _filter.MinimalnaOcjena = minimalnaOcjena;
            return this;
        }

        public ContentSearchFilterBuilder WithSort(string? sort)
        {
            _filter.Sort = sort;
            return this;
        }

        public ContentSearchFilter Build() => _filter;
    }
}
