using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Mappings
{
    public class AuthMapper : IAuthMapper
    {
        public User ToUser(RegisterRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
