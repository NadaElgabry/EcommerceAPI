using EcommerceAPI.Application.Common;
using EcommerceAPI.Application.Interfaces.Search;
using EcommerceAPI.Infrastructure.Services.Search.Indexing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductIndexController : ControllerBase
    {
        private readonly IProductIndexingService _indexingService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProductIndexController> _logger;

        public ProductIndexController(
            IProductIndexingService indexingService,
            IServiceProvider serviceProvider,
            ILogger<ProductIndexController> logger)
        {
            _indexingService = indexingService;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        /// <summary>
        /// Re-syncs Elasticsearch data from the database into the existing index.
        /// Does not touch the mapping. Safe to run any time.
        /// </summary>
        [HttpPost("reindex")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReindexAll(CancellationToken cancellationToken)
        {
                await _indexingService.ReindexAllProductsAsync(cancellationToken);

                _logger.LogInformation("Product reindex completed successfully.");

                return Ok(ApiResponse<string>.SuccessResponse(
                    statusCode: 200,
                   message: "Product reindex completed."));
        }

        /// <summary>
        /// Drops and recreates the products index with the current mapping/analyzers,
        /// then reindexes all data. Use only after a mapping change deploy — the index
        /// is briefly unavailable for search during this operation.
        /// </summary>
        [HttpPost("recreate-index")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RecreateIndex(CancellationToken cancellationToken)
        {
                await _serviceProvider.RecreateProductsIndexAsync();
                await _indexingService.ReindexAllProductsAsync(cancellationToken);

                _logger.LogInformation("Product index recreated and reindexed successfully.");

                return Ok(ApiResponse<string>.SuccessResponse(
                    statusCode: 200,
                    message: "Product index recreated and reindexed."));
        }

    }
}
