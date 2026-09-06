namespace EcommerceAPI.Infrastructure.Settings
{
    public class ElasticsearchSettings
    {
        public required string Url { get; set; }

        public required string ProductsIndex { get; set; }

        /// <summary>Fields for prefix / partial-character matching (edge-ngram subfields).</summary>
        public string[] ProductPrefixFields { get; set; } = Array.Empty<string>();

        /// <summary>Fields for word-family matching (stemmed + synonym subfields).</summary>
        public string[] ProductSemanticFields { get; set; } = Array.Empty<string>();

        /// <summary>Fields for exact/near-exact matching with typo tolerance (fuzziness).</summary>
        public string[] ProductExactFields { get; set; } = Array.Empty<string>();
    }
}