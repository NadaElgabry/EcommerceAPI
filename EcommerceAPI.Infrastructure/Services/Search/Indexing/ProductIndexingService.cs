using EcommerceAPI.Application.Interfaces.Repositories;
using EcommerceAPI.Application.Interfaces.Search;
using EcommerceAPI.Domain.Entities;
using EcommerceAPI.Infrastructure.Services.Search.Documents;
using EcommerceAPI.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace EcommerceAPI.Infrastructure.Services.Search.Indexing
{
    public class ProductIndexingService : IProductIndexingService
    {
        private const int BatchSize = 500;

        private readonly IRepository<Product> _productRepository;
        private readonly ISearchService<ProductSearchDocument> _search;
        private readonly ElasticsearchSettings _settings;

        public ProductIndexingService(
            IRepository<Product> productRepository,
            ISearchService<ProductSearchDocument> search,
            IOptions<ElasticsearchSettings> settings)
        {
            _productRepository = productRepository;
            _search = search;
            _settings = settings.Value;
        }

        public async Task ReindexAllProductsAsync(CancellationToken cancellationToken = default)
        {
            int? lastId = null;

            while (true)
            {
                var batch = await _productRepository.GetPagedAsync(
                    predicate: lastId == null
                        ? (Expression<Func<Product, bool>>)(p => true)
                        : p => p.Id > lastId,
                    orderBy: p => p.Id,
                    take: BatchSize,
                    include: q => q
                        .Include(p => p.Category)
                        .Include(p => p.ProductTags)
                            .ThenInclude(pt => pt.Tag),
                    cancellationToken: cancellationToken);

                if (batch.Count == 0)
                {
                    break;
                }

                var documents = batch.Select(p => (p.Id.ToString(), MapToDocument(p)));

                await _search.IndexManyAsync(_settings.ProductsIndex, documents, cancellationToken);

                lastId = batch[^1].Id;

                if (batch.Count < BatchSize)
                {
                    break; 
                }
            }
        }

        public async Task IndexProductAsync(Product product, CancellationToken cancellationToken = default)
        {
            var document = MapToDocument(product);
            await _search.IndexOneAsync(_settings.ProductsIndex, product.Id.ToString(), document, cancellationToken);
        }

        public async Task DeleteProductAsync(int productId, CancellationToken cancellationToken = default)
        {
            await _search.DeleteOneAsync(_settings.ProductsIndex, productId.ToString(), cancellationToken);
        }

        private static ProductSearchDocument MapToDocument(Product product)
        {
            return new ProductSearchDocument
            {
                Id = product.Id,
                Slug = product.Slug,
                Name = product.Name,
                Description = product.Description,
                Brand = product.Brand,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ProductImage = product.ProductImage,
                AltText = product.AltText,
                CreationDate = product.CreationDate,
                CategorySlug = product.Category.Slug,
                Tags = product.ProductTags.Select(pt => pt.Tag.Slug).ToList()
            };
        }
    }
}