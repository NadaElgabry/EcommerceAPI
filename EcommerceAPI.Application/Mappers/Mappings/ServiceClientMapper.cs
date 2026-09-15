using EcommerceAPI.Application.DTOs.ServiceAuth;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers
{
    public class ServiceClientMapper : IServiceClientMapper
    {
        public ServiceClient ToEntity(CreateServiceClientRequest request, string clientId, string clientSecretHash) =>
            new()
            {
                ClientId = clientId,
                ClientSecretHash = clientSecretHash,
                Name = request.Name,
                ScopesCsv = string.Join(',', request.Scopes)
            };

        public CreateServiceClientResponse ToCreateResponse(ServiceClient entity, string rawSecret) =>
            new()
            {
                ClientId = entity.ClientId,
                ClientSecret = rawSecret
            };
    }
}