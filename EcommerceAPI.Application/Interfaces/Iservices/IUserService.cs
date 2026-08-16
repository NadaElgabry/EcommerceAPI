using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Iservices
{
    public interface IUserService
    {
        /// <summary>
        /// Retrieves the profile information of a user.
        /// </summary>
        /// <param name="guid">The unique identifier of the user.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Profile of a user</returns>
        public Task<UserResponse> GetUserProfileAsync(Guid guid,CancellationToken cancellationToken);
    }
}
