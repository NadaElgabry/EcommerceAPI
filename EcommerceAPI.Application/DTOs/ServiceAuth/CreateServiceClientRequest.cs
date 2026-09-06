namespace EcommerceAPI.Application.DTOs.ServiceAuth
{
    public class CreateServiceClientRequest
    {
        public string Name { get; set; } = default!;
        public List<string> Scopes { get; set; } = default!;
    }
}