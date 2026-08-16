using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface IUserMapper
    {

        /// <summary>
        /// Maps a User entity to a UserResponse DTO.
        /// </summary>
        /// <param name="user">The User entity to map.</param>
        /// <returns>The mapped UserResponse DTO.</returns>
        public UserResponse ToUserResponse(User user);
        public void UpdateUserFromRequest(User user, UpdateProfileRequest request);
    }
}
