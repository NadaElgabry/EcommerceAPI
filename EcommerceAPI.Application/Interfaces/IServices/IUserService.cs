using EcommerceAPI.Application.DTOs.Auth;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceAPI.Application.Interfaces.IServices
{
    public interface IUserService
    {

        /// <summary>
        /// Retrieves the profile information of a user.
        /// </summary>
        /// <param name="guid">The unique identifier of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Profile of a user</returns>
        public Task<UserResponse> GetUserProfileAsync(Guid guid, CancellationToken cancellationToken);
        public Task UpdateProfileAsync(
           Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
        public Task<PagedResult<UserResponse>> GetAllUsersAsync(
            GetUsersRequest request, CancellationToken cancellationToken = default);

    }
}
