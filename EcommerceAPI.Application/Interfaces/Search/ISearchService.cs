// Application/Interfaces/Search/ISearchService.cs — no Elastic references at all
using EcommerceAPI.Application.DTOs.Common;

namespace EcommerceAPI.Application.Interfaces.Search
{
    public interface ISearchService<TDocument> where TDocument : class
    {
        /// <summary>
        /// Searches the specified index using the given search request.
        /// </summary>
        /// <param name="indexName">The name of the index to search.</param>
        /// <param name="request">The search request containing query and pagination parameters.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A cursor-paged result of matching documents.</returns>
        Task<CursorPagedResult<TDocument>> SearchAsync(string indexName, SearchRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Indexes a single document in the specified index.
        /// </summary>
        /// <param name="indexName">The name of the index to write to.</param>
        /// <param name="id">The unique identifier of the document.</param>
        /// <param name="document">The document to index.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        Task IndexOneAsync(string indexName, string id, TDocument document, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a single document from the specified index.
        /// </summary>
        /// <param name="indexName">The name of the index to delete from.</param>
        /// <param name="id">The unique identifier of the document to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        Task DeleteOneAsync(string indexName, string id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Indexes multiple documents in the specified index.
        /// </summary>
        /// <param name="indexName">The name of the index to write to.</param>
        /// <param name="documents">The collection of documents to index, each paired with its unique identifier.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        Task IndexManyAsync(string indexName, IEnumerable<(string Id, TDocument Document)> documents, CancellationToken cancellationToken = default);
    }
}