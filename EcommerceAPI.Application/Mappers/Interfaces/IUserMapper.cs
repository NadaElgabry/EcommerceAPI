using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface IUserMapper
    {
        public void UpdateUserFromRequest(User user, UpdateProfileRequest request);
    }
}
