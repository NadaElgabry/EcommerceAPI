using EcommerceAPI.Application.DTOs.Auth;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IUsersService
    {
        public Task UpdateProfileAsync(
           Guid? userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);

    }
}
