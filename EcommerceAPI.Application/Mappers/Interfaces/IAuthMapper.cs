using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Mappers.Interfaces
{
    public interface IAuthMapper
    {
        public User ToUser(RegisterRequest request);
    }
}
