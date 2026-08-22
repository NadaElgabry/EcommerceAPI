// Application/Interfaces/Search/ISearchService.cs — no Elastic references at all
using EcommerceAPI.Application.DTOs.Common;

namespace EcommerceAPI.Application.Interfaces.Search
{
    public interface ISearchService<TDocument> where TDocument : class
    {
        Task<CursorPagedResult<TDocument>> SearchAsync(string indexName, SearchRequest request, CancellationToken cancellationToken = default);
        Task IndexOneAsync(string indexName, string id, TDocument document, CancellationToken cancellationToken = default);
        Task DeleteOneAsync(string indexName, string id, CancellationToken cancellationToken = default);
        Task IndexManyAsync(string indexName, IEnumerable<(string Id, TDocument Document)> documents, CancellationToken cancellationToken = default);
    }
}