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
        public UserResponse ToUserResponse(User user)
        {
            return new UserResponse
            {
                Guid = user.Guid,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
