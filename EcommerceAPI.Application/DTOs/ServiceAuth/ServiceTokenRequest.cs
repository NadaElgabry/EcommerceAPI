namespace EcommerceAPI.Application.DTOs.ServiceAuth
{
    public class ServiceTokenRequest
    {
        public string ClientId { get; set; } = default!;
        public string ClientSecret { get; set; } = default!;
    }
}