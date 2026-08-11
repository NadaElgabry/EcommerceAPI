using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.Auth
{
    public class UserResponse
    {
        public Guid Guid { get; set; } = Guid.NewGuid();

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
