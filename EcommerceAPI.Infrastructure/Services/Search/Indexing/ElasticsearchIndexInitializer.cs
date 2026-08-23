using EcommerceAPI.Infrastructure.Services.Search.Documents;
using EcommerceAPI.Infrastructure.Settings;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Infrastructure.Services.Search.Indexing
{
    public static class ElasticsearchIndexInitializer
    {
        /// <summary>
        /// Ensures the Elasticsearch products index exists, creating it with the required
        /// analyzers and mappings if it does not already exist.
        /// </summary>
        /// <param name="services">The service provider used to resolve the Elasticsearch client and settings.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the index creation request fails.
        /// </exception>
        public static async Task EnsureProductsIndexExistsAsync(this IServiceProvider services)
        {
            var client = services.GetRequiredService<ElasticsearchClient>();
            var settings = services.GetRequiredService<IOptions<ElasticsearchSettings>>().Value;

            var existsResponse = await client.Indices.ExistsAsync(settings.ProductsIndex);
            if (existsResponse.Exists)
            {
                return;
            }

            var createResponse = await client.Indices.CreateAsync<ProductSearchDocument>(
                settings.ProductsIndex,
                c => c
                    .Settings(s => s
                        .Analysis(a => a
                            .Tokenizers(t => t
                                .EdgeNGram("edge_ngram_tokenizer", e => e
                                    .MinGram(2)
                                    .MaxGram(15)
                                    .TokenChars(new[] { TokenChar.Letter, TokenChar.Digit })
                                )
                            )
                            .Analyzers(an => an
                                .Custom("edge_ngram_analyzer", ca => ca
                                    .Tokenizer("edge_ngram_tokenizer")
                                    .Filter(new[] { "lowercase" })
                                )
                            )
                        )
                    )
                    .Mappings(m => m
                        .Properties(p => p
                            .IntegerNumber(d => d.Id)
                            .Keyword(d => d.Slug)
                            .Text(d => d.Name, t => t
                                .Fields(f => f
                                    .Keyword("keyword")
                                    .Text("ngram", tt => tt
                                        .Analyzer("edge_ngram_analyzer")
                                        .SearchAnalyzer("standard"))
                                ))
                            .Text(d => d.Description)
                            .Text(d => d.Brand, t => t
                                .Fields(f => f.Keyword("keyword")))
                            .DoubleNumber(d => d.Price)
                            .IntegerNumber(d => d.StockQuantity)
                            .Keyword(d => d.ProductImage, k => k.Index(false))
                            .Keyword(d => d.AltText, k => k.Index(false))
                            .Date(d => d.CreationDate)
                            .Keyword(d => d.CategorySlug)
                            .Text(d => d.Tags, t => t
                                .Fields(f => f.Keyword("keyword")))
                        )
                    )
            );

            if (!createResponse.IsValidResponse)
            {
                throw new InvalidOperationException(
                    $"Failed to create Elasticsearch index '{settings.ProductsIndex}': {createResponse.DebugInformation}");
            }
        }
    }
}