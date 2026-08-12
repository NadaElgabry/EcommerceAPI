using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface IAuthMapper
    {
        /// <summary>
        /// Maps a RegisterRequest object to a User entity.
        /// </summary>
        /// <param name="request">The register request.</param>
        /// <returns>The user entity.</returns>
        public User ToUser(RegisterRequest request);
        
    }
}
