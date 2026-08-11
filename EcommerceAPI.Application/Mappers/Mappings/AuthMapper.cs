using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Mappings
{
    public class AuthMapper : IAuthMapper
    {
        public User ToUser(RegisterRequest request)
        {
            return new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber
            };

        }
        public UserResponse ToUserResponse(User user)
        {
            return new UserResponse
            {
                Guid = user.Guid,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
