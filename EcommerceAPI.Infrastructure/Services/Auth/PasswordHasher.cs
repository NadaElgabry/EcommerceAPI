using EcommerceAPI.Application.Interfaces.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Infrastructure.Services.Auth
{
    public class PasswordHasher : IPasswordHasher
    {
        /// <inheritdoc />
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        /// <inheritdoc />  
        public bool Verify(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }

    }
}
