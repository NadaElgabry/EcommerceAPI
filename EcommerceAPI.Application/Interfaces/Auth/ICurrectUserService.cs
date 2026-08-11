using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Auth
{
    public interface ICurrentUserService
    {
        public Guid UserGuid { get; }
        public string? Role { get; }
        public bool IsAuthenticated { get; }
    }
}