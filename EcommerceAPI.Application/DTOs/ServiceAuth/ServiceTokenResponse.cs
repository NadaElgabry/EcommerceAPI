namespace EcommerceAPI.Application.DTOs.ServiceAuth
{
    public class ServiceTokenResponse
    {
        public string AccessToken { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
    }
}