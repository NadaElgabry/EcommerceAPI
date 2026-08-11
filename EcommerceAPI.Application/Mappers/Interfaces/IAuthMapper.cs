using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface IAuthMapper
    {
        public User ToUser(RegisterRequest request);
        public void UpdateUserFromRequest(User user, UpdateProfileRequest request);
    }
}
