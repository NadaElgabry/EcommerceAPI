using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.Exceptions;
using EcommerceAPI.Application.Interfaces.Search;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using System.Security.Cryptography;
using System.Text;
using SearchRequest = EcommerceAPI.Application.Interfaces.Search.SearchRequest;

namespace EcommerceAPI.Infrastructure.Services.Search
{
    public class ElasticSearchService<TDocument> : ISearchService<TDocument>
        where TDocument : class
    {
        private readonly ElasticsearchClient _client;

        public ElasticSearchService(ElasticsearchClient client)
        {
            _client = client;
        }

        ///<inheritdoc/>
        public async Task<CursorPagedResult<TDocument>> SearchAsync(
            string indexName,
            SearchRequest request,
            CancellationToken cancellationToken = default)
        {
            var hasSearchText = !string.IsNullOrWhiteSpace(request.SearchText);
            var searchText = request.SearchText ?? string.Empty;

            var sortField = string.IsNullOrWhiteSpace(request.SortField)
                ? (hasSearchText ? "_score" : "id")
                : request.SortField;

            var sortOrder = request.SortDir == SearchSortDirection.Asc ? SortOrder.Asc : SortOrder.Desc;

            var fingerprint = ComputeQueryFingerprint(request);

            SearchCursorPayload? cursor = null;
            if (!string.IsNullOrWhiteSpace(request.Cursor))
            {
                cursor = CursorHelper.Decode<SearchCursorPayload>(request.Cursor);

                var expectedSortBy = sortField ?? "_score";
                if (!string.Equals(cursor.SortBy, expectedSortBy, StringComparison.OrdinalIgnoreCase))
                {
                    throw new BadRequestException(
                        "Cursor does not match the current sort. Restart pagination from the first page.");
                }

                if (!string.Equals(cursor.QueryFingerprint, fingerprint, StringComparison.Ordinal))
                {
                    throw new BadRequestException(
                        "Cursor does not match the current search text or filters. Restart pagination from the first page.");
                }
            }

            var response = await _client.SearchAsync<TDocument>(s =>
            {
                s.Indices(indexName)
                    .Size(request.Limit + 1)
                    .Query(q => q.Bool(b =>
                    {
                        var filterActions = BuildFilters(request.Filters);
                        if (filterActions.Count > 0)
                        {
                            b.Filter(filterActions.ToArray());
                        }

                        if (hasSearchText && request.SearchFields is { Length: > 0 })
                        {
                            var fuzzyFields = request.SearchFields
                                .Where(f => !f.StartsWith("description", StringComparison.OrdinalIgnoreCase))
                                .ToArray();
                            var exactFields = request.SearchFields
                                .Where(f => f.StartsWith("description", StringComparison.OrdinalIgnoreCase))
                                .ToArray();

                            b.Must(m => m.Bool(inner =>
                            {
                                inner.MinimumShouldMatch(1);

                                if (fuzzyFields.Length > 0)
                                {
                                    inner.Should(sh => sh.MultiMatch(mm => mm
                                        .Query(searchText)
                                        .Fields(fuzzyFields)
                                        .Type(TextQueryType.BestFields)
                                        .Fuzziness(new Fuzziness("AUTO"))
                                        .PrefixLength(2)
                                        .MinimumShouldMatch("75%")));
                                }

                                if (exactFields.Length > 0)
                                {
                                    inner.Should(sh => sh.MultiMatch(mm => mm
                                        .Query(searchText)
                                        .Fields(exactFields)
                                        .Type(TextQueryType.BestFields)));
                                }
                            }));
                        }
                    }))
                    .Sort(
                        so =>
                        {
                            if (sortField == "_score")
                            {
                                so.Score(sc => sc.Order(SortOrder.Desc));
                            }
                            else
                            {
                                so.Field(f => f.Field(sortField).Order(sortOrder));
                            }
                        },
                        so => so.Field(f => f.Field("id").Order(SortOrder.Asc)) // tiebreaker
                    );

                if (cursor != null)
                {
                    s.SearchAfter(cursor.Values.Select(v => FieldValue.String(v)).ToList());
                }
            }, cancellationToken);

            if (!response.IsValidResponse)
            {
                throw new InvalidOperationException($"Search on '{indexName}' failed: {response.DebugInformation}");
            }

            var allHits = response.Hits.ToList();
            var hasNext = allHits.Count > request.Limit;
            var pageHits = hasNext ? allHits.Take(request.Limit).ToList() : allHits;

            var items = pageHits.Select(h => h.Source!).ToList();

            string? nextCursor = null;
            if (hasNext && pageHits.Count > 0)
            {
                var lastSortValues = pageHits[^1].Sort!
                    .Select(v => v.ToString() ?? string.Empty)
                    .ToArray();

                nextCursor = CursorHelper.Encode(new SearchCursorPayload
                {
                    SortBy = sortField ?? "_score",
                    Values = lastSortValues,
                    QueryFingerprint = fingerprint
                });
            }

            return new CursorPagedResult<TDocument>
            {
                Data = items,
                Pagination = new CursorPageInfo
                {
                    NextCursor = nextCursor,
                    HasNext = hasNext,
                    PageSize = items.Count
                }
            };
        }

        ///<inheritdoc/>
        public async Task IndexOneAsync(string indexName, string id, TDocument document, CancellationToken cancellationToken = default)
        {
            var response = await _client.IndexAsync(document, i => i.Index(indexName).Id(id), cancellationToken);

            if (!response.IsValidResponse)
            {
                throw new InvalidOperationException($"Indexing document '{id}' in '{indexName}' failed: {response.DebugInformation}");
            }
        }
        
        ///<inheritdoc/>
        public async Task DeleteOneAsync(string indexName, string id, CancellationToken cancellationToken = default)
        {
            var response = await _client.DeleteAsync<TDocument>(id, d => d.Index(indexName), cancellationToken);

            if (!response.IsValidResponse && response.ApiCallDetails.HttpStatusCode != 404)
            {
                throw new InvalidOperationException($"Deleting document '{id}' from '{indexName}' failed: {response.DebugInformation}");
            }
        }

        ///<inheritdoc/>
        public async Task IndexManyAsync(
            string indexName,
            IEnumerable<(string Id, TDocument Document)> documents,
            CancellationToken cancellationToken = default)
        {
            var docsList = documents.ToList();
            if (docsList.Count == 0)
            {
                return;
            }

            var response = await _client.BulkAsync(b =>
            {
                b.Index(indexName);
                foreach (var (id, document) in docsList)
                {
                    b.Index<TDocument>(document, idx => idx.Id(id));
                }
            }, cancellationToken);

            if (!response.IsValidResponse)
            {
                throw new InvalidOperationException($"Bulk indexing into '{indexName}' failed: {response.DebugInformation}");
            }

            if (response.Errors)
            {
                var failedItems = response.ItemsWithErrors.ToList();

                var errorDetails = string.Join("; ", failedItems.Select(i =>
                    $"id={i.Id}, status={i.Status}, error={i.Error?.Reason ?? "unknown"}"));

                throw new InvalidOperationException(
                    $"Bulk indexing into '{indexName}' failed for {failedItems.Count} of {docsList.Count} " +
                    $"document(s): {errorDetails}");
            }
        }

        /// <summary>
        /// Converts a list of generic <see cref="SearchFilter"/> definitions into a list of
        /// Elasticsearch query descriptor actions that can be combined into a bool query.
        /// </summary>
        /// <param name="filters">The filters to translate into Elasticsearch query clauses.</param>
        /// <returns>
        /// A list of actions, each configuring a <see cref="QueryDescriptor{TDocument}"/> for one filter.
        /// </returns>
        private static List<Action<QueryDescriptor<TDocument>>> BuildFilters(List<SearchFilter> filters)
        {
            var actions = new List<Action<QueryDescriptor<TDocument>>>();

            foreach (var filter in filters)
            {
                switch (filter.Type)
                {
                    case SearchFilterType.Term:
                        var termValue = ToFieldValue(filter.Value);
                        actions.Add(q => q.Term(t => t.Field(filter.Field).Value(termValue)));
                        break;

                    case SearchFilterType.Terms:
                        var termsValues = (filter.Values ?? Enumerable.Empty<object>())
                            .Select(ToFieldValue)
                            .ToArray();
                        actions.Add(q => q.Terms(t => t
                            .Field(filter.Field)
                            .Terms(new TermsQueryField(termsValues))));
                        break;

                    case SearchFilterType.RangeGte:
                        var gteValue = ToDouble(filter.Value);
                        actions.Add(q => q.Range(r => r.Number(nr => nr.Field(filter.Field).Gte(gteValue))));
                        break;

                    case SearchFilterType.RangeLte:
                        var lteValue = ToDouble(filter.Value);
                        actions.Add(q => q.Range(r => r.Number(nr => nr.Field(filter.Field).Lte(lteValue))));
                        break;

                    case SearchFilterType.RangeGt:
                        var gtValue = ToDouble(filter.Value);
                        actions.Add(q => q.Range(r => r.Number(nr => nr.Field(filter.Field).Gt(gtValue))));
                        break;

                    case SearchFilterType.RangeLt:
                        var ltValue = ToDouble(filter.Value);
                        actions.Add(q => q.Range(r => r.Number(nr => nr.Field(filter.Field).Lt(ltValue))));
                        break;
                }
            }

            return actions;
        }

        /// <summary>
        /// Converts a raw filter value into the <see cref="FieldValue"/> type expected by the
        /// Elasticsearch client, mapping common .NET primitive types to their corresponding
        /// Elasticsearch representations.
        /// </summary>
        /// <param name="value">The raw value to convert. Supported types are string, bool, int, long, double, and decimal.</param>
        /// <returns>
        /// The equivalent <see cref="FieldValue"/>. Unsupported types fall back to their string representation.
        /// </returns>
        private static FieldValue ToFieldValue(object? value) => value switch
        {
            null => FieldValue.Null,
            string s => FieldValue.String(s),
            bool b => FieldValue.Boolean(b),
            int i => FieldValue.Long(i),
            long l => FieldValue.Long(l),
            double d => FieldValue.Double(d),
            decimal dec => FieldValue.Double((double)dec),
            _ => FieldValue.String(value.ToString() ?? string.Empty)
        };

        /// <summary>
        /// Converts a raw filter value into a <see cref="double"/> for use in numeric range queries.
        /// </summary>
        /// <param name="value">The raw value to convert. Supported types are double, int, long, and decimal.</param>
        /// <returns>
        /// The value converted to a <see cref="double"/>, or 0 if <paramref name="value"/> is null.
        /// Unsupported types are converted using <see cref="Convert.ToDouble(object)"/>.
        /// </returns>
        private static double ToDouble(object? value) => value switch
        {
            null => 0,
            double d => d,
            int i => i,
            long l => l,
            decimal dec => (double)dec,
            _ => Convert.ToDouble(value)
        };

        /// <summary>
        /// Computes a deterministic fingerprint for a search request by hashing its normalized
        /// search text and filters. Used to identify identical queries (e.g. for caching), since
        /// requests with the same effective search criteria produce the same fingerprint regardless
        /// of filter ordering or value casing.
        /// </summary>
        /// <param name="request">The search request to compute a fingerprint for.</param>
        /// <returns>A 16-character hexadecimal string derived from the SHA-256 hash of the normalized request.</returns>
        private static string ComputeQueryFingerprint(SearchRequest request)
        {
            var sb = new StringBuilder();
            sb.Append(request.SearchText?.Trim().ToLowerInvariant());
            sb.Append('|');

            foreach (var f in request.Filters.OrderBy(f => f.Field, StringComparer.Ordinal)
                                              .ThenBy(f => f.Type))
            {
                sb.Append(f.Field).Append(':').Append(f.Type).Append('=');

                if (f.Values != null)
                {
                    var sortedValues = f.Values.Select(v => v?.ToString() ?? string.Empty)
                                                .OrderBy(v => v, StringComparer.Ordinal);
                    sb.Append(string.Join(",", sortedValues));
                }
                else
                {
                    sb.Append(f.Value?.ToString() ?? string.Empty);
                }

                sb.Append(';');
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash)[..16];
        }
    }
}