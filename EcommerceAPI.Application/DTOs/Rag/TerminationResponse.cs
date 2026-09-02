using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace EcommerceAPI.Application.DTOs.Rag
{
    public class TerminationResponse
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = null!;

        [JsonPropertyName("suggested_products")]
        public List<int> SuggestedProducts { get; set; } = new();
    }
}
