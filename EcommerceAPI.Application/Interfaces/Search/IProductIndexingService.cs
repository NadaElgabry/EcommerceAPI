using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Search
{
    public interface IProductIndexingService
    {
        /// <summary>
        /// Reindexes all products in Elasticsearch.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        Task ReindexAllProductsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Indexes an individual product in Elasticsearch.
        /// </summary>
        /// <param name="product">The product to be indexed.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        Task IndexProductAsync(Product product, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a product from the Elasticsearch index.
        /// </summary>
        /// <param name="productId">The id of the product to delete.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        Task DeleteProductAsync(int productId, CancellationToken cancellationToken = default);
    }
}
