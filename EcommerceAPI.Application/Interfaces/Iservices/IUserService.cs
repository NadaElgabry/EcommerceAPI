using EcommerceAPI.Application.DTOs.Common;
using EcommerceAPI.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Iservices
{
    public interface IUserService
    {
        public Task<PagedResult<UserResponse>> GetAllUsersAsync(
    GetUsersRequest request, CancellationToken cancellationToken = default);
    }
}
