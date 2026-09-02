using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Product;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.Search;
using EcommerceAPI.Infrastructure.Services.Search.Documents;
using EcommerceAPI.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Infrastructure.Services.Search
{
    public class ElasticProductSearchService : IProductSearchService
    {
        private readonly ISearchService<ProductSearchDocument> _search;
        private readonly ElasticsearchSettings _settings;
        private readonly ILogger<ElasticProductSearchService> _logger;

        public ElasticProductSearchService(
            ISearchService<ProductSearchDocument> search,
            IOptions<ElasticsearchSettings> settings,
            ILogger<ElasticProductSearchService> logger)
        {
            _search = search;
            _settings = settings.Value;
            _logger = logger;
        }

        ///<inheritdoc/>
        public async Task<CursorPagedResult<ProductSummaryResponse>> SearchProductsAsync(
            ProductQueryParamsRequest queryParams,
            CancellationToken cancellationToken = default)
        {
            var hasSearchText = !string.IsNullOrWhiteSpace(queryParams.Search);

            var request = new SearchRequest
            {
                SearchText = queryParams.Search,
                PrefixFields = _settings.ProductPrefixFields,
                SemanticFields = _settings.ProductSemanticFields,
                ExactFields = _settings.ProductExactFields,
                SortField = ResolveSortField(queryParams.SortBy, hasSearchText),
                SortDir = string.Equals(queryParams.SortDir, "asc", StringComparison.OrdinalIgnoreCase)
                    ? SearchSortDirection.Asc
                    : SearchSortDirection.Desc,
                Limit = Math.Clamp(queryParams.Limit <= 0 ? 20 : queryParams.Limit, 1, 100),
                Cursor = queryParams.Cursor,
                Filters = BuildFilters(queryParams)
            };
            var result = await _search.SearchAsync(_settings.ProductsIndex, request, cancellationToken);

            var items = result.Data.Select(doc => new ProductSummaryResponse
            {
                Name = doc.Name,
                Slug = doc.Slug,
                Price = doc.Price,
                StockQuantity = doc.StockQuantity,
                ProductImageUrl = doc.ProductImage,
                AltText = doc.AltText,
                CategorySlug = doc.CategorySlug
            }).ToList();
            _logger.LogInformation("Prefix: {P} | Semantic: {S} | Exact: {E}",
                string.Join(",", request.PrefixFields ?? Array.Empty<string>()),
                string.Join(",", request.SemanticFields ?? Array.Empty<string>()),
                string.Join(",", request.ExactFields ?? Array.Empty<string>()));
            return new CursorPagedResult<ProductSummaryResponse>
            {
                Data = items,
                Pagination = result.Pagination
            };
        }

        private static readonly Dictionary<string, string> SortFieldMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["name"] = "name.keyword",
            ["price"] = "price",
            ["newest"] = "creationDate",
            ["stock"] = "stockQuantity"
        };

        /// <summary>
        /// Resolves the Elasticsearch field name to sort by, based on the requested sort key.
        /// When no sort key is requested, defaults to sorting by creation date unless a search
        /// text is present, in which case relevance scoring is used instead (no explicit sort field).
        /// </summary>
        /// <param name="requestedSortBy">The sort key requested by the caller, or null/empty to use the default.</param>
        /// <param name="hasSearchText">Whether the search request includes free-text search terms.</param>
        /// <returns>
        /// The mapped Elasticsearch field name to sort by, "creationDate" if no sort was requested and there
        /// is no search text, or <see langword="null"/> if no sort was requested and a search text is present
        /// (allowing results to be ordered by relevance score).
        /// </returns>
        /// <exception cref="BadRequestException">
        /// Thrown when <paramref name="requestedSortBy"/> does not match any allowed sort field.
        /// </exception>
        private static string? ResolveSortField(string? requestedSortBy, bool hasSearchText)
        {
            if (string.IsNullOrWhiteSpace(requestedSortBy))
            {
                return hasSearchText ? null : "creationDate";
            }

            if (SortFieldMap.TryGetValue(requestedSortBy, out var mapped))
            {
                return mapped;
            }

            throw new BadRequestException(
                $"Invalid sortBy value '{requestedSortBy}'. Allowed values: {string.Join(", ", SortFieldMap.Keys)}.");
        }

        /// <summary>
        /// Builds the list of Elasticsearch filters to apply based on the provided query parameters.
        /// Supports filtering by category, tags, price range, and stock availability.
        /// </summary>
        /// <param name="queryParams">The query parameters containing the requested filter criteria.</param>
        /// <returns>A list of <see cref="SearchFilter"/> instances representing the filters to apply.</returns>
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