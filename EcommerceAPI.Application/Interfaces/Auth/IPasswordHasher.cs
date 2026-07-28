using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Auth
{
    public interface IPasswordHasher
    {
        public string Hash(string password);

        public bool Verify(string password, string hashedPassword);

    }
}
