using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Search
{
    public interface IProductIndexingService
    {
        Task ReindexAllProductsAsync(CancellationToken cancellationToken = default);
        Task IndexProductAsync(Product product, CancellationToken cancellationToken = default);
        Task DeleteProductAsync(int productId, CancellationToken cancellationToken = default);
    }
}
