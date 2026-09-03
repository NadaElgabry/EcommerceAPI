using EcommerceAPI.Application.DTOs.ServiceAuth;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface IServiceClientMapper
    {
        ServiceClient ToEntity(CreateServiceClientRequest request, string clientId, string clientSecretHash);
        CreateServiceClientResponse ToCreateResponse(ServiceClient entity, string rawSecret);
    }
}