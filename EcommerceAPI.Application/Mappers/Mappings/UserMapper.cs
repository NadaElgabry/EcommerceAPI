using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Mappings
{
    public class UserMapper : IUserMapper
    {
        ///<inheritdoc/>
        public UserResponse ToUserResponse(User user)
        {
            return new UserResponse
            {
                Guid = user.Guid,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.isActive,
                CreatedAt = user.CreatedAt
            };
        }

        public void UpdateUserFromRequest(User user, UpdateProfileRequest request)
        {
            if (request.FirstName != null) user.FirstName = request.FirstName;
            if (request.LastName != null) user.LastName = request.LastName;
            if (request.PhoneNumber != null) user.PhoneNumber = request.PhoneNumber;
        }
    }
}