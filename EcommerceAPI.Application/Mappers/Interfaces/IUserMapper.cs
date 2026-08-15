using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface IUserMapper
    {
        public UserResponse ToUserResponse(User user);
    }
}