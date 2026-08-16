using EcommerceAPI.Application.DTOs.User;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Mappers.Mappings
{
    public class UserMapper : IUserMapper
    {

        ///<inheritdoc/>
        public UserResponse ToUserResponse(User user)
        {
            return new UserResponse
            {
                UserId = user.Guid,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role.ToString(),
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
