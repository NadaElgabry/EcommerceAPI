using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Mappers.Interfaces;
using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
