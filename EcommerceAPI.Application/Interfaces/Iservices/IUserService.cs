using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Iservices
{
    public interface IUserService
    {
        public Task<PagedResult<UserResponse>> GetUsersAsync(
    GetUsersRequest request, CancellationToken cancellationToken = default);
    }
}
