using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface IUsersMapper
    {
        public void UpdateUserFromRequest(User user, UpdateProfileRequest request);
    }
}
