using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.Interfaces.Search;
using EcommerceAPI.Infrastructure.Services.Search.Documents;
using EcommerceAPI.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Infrastructure.Services.Search
{
    public class ElasticProductSearchService : IProductSearchService
    {
        private readonly ISearchService<ProductSearchDocument> _search;
        private readonly ElasticsearchSettings _settings;

        public ElasticProductSearchService(
            ISearchService<ProductSearchDocument> search,
            IOptions<ElasticsearchSettings> settings)
        {
            _search = search;
            _settings = settings.Value;
        }

        public async Task<CursorPagedResult<ProductSummaryResponse>> SearchProductsAsync(
            ProductQueryParamsRequest queryParams,
            CancellationToken cancellationToken = default)
        {
            var hasSearchText = !string.IsNullOrWhiteSpace(queryParams.Search);

            var request = new SearchRequest
            {
                SearchText = queryParams.Search,
                SearchFields = new[] { "name^3", "name.ngram^1", "brand^2", "tags^1.5", "description" },
                SortField = string.IsNullOrWhiteSpace(queryParams.SortBy)
                    ? (hasSearchText ? "_score" : "creationDate")
                    : queryParams.SortBy,
                SortDir = string.Equals(queryParams.SortDir, "asc", StringComparison.OrdinalIgnoreCase)
                    ? SearchSortDirection.Asc
                    : SearchSortDirection.Desc,
                Limit = queryParams.Limit,
                Cursor = queryParams.Cursor,
                Filters = BuildFilters(queryParams)
            };

            var result = await _search.SearchAsync(_settings.ProductsIndex, request, cancellationToken);

            var items = result.Data.Select(doc => new ProductSummaryResponse
            {
                Name = doc.Name,
                Slug = doc.Slug,
                Price = doc.Price,
                ProductImageUrl = doc.ProductImage,
                AltText = doc.AltText
            }).ToList();

            return new CursorPagedResult<ProductSummaryResponse>
            {
                Data = items,
                Pagination = result.Pagination
            };
        }

        private static List<SearchFilter> BuildFilters(ProductQueryParamsRequest queryParams)
        {
            var filters = new List<SearchFilter>();

            if (!string.IsNullOrWhiteSpace(queryParams.CategorySlug))
            {
                filters.Add(new SearchFilter
                {
                    Field = "categorySlug",
                    Type = SearchFilterType.Term,
                    Value = queryParams.CategorySlug
                });
            }

            if (queryParams.Tags is { Count: > 0 })
            {
                filters.Add(new SearchFilter
                {
                    Field = "tags.keyword",
                    Type = SearchFilterType.Terms,
                    Values = queryParams.Tags.Cast<object>()
                });
            }

            if (queryParams.Brand is { Count: > 0 })
            {
                filters.Add(new SearchFilter
                {
                    Field = "brand.keyword",
                    Type = SearchFilterType.Terms,
                    Values = queryParams.Brand.Cast<object>()
                });
            }

            if (queryParams.MinPrice.HasValue)
            {
                filters.Add(new SearchFilter
                {
                    Field = "price",
                    Type = SearchFilterType.RangeGte,
                    Value = (double)queryParams.MinPrice.Value
                });
            }

            if (queryParams.MaxPrice.HasValue)
            {
                filters.Add(new SearchFilter
                {
                    Field = "price",
                    Type = SearchFilterType.RangeLte,
                    Value = (double)queryParams.MaxPrice.Value
                });
            }

            if (queryParams.InStock == true)
            {
                filters.Add(new SearchFilter
                {
                    Field = "stockQuantity",
                    Type = SearchFilterType.RangeGt,
                    Value = 0
                });
            }

            return filters;
        }
    }
}