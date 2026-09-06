namespace EcommerceAPI.Application.DTOs.ServiceAuth
{
    public class UpdateServiceClientScopesRequest
    {
        public List<string> Scopes { get; set; } = default!;
    }
}