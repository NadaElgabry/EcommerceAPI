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

        public void UpdateUserFromRequest(User user, UpdateProfileRequest request)
        {
            if (request.FirstName != null)  user.FirstName = request.FirstName;
            if (request.LastName != null)   user.LastName = request.LastName;
            if (request.PhoneNumber != null)    user.PhoneNumber = request.PhoneNumber;
        }
    }
}
