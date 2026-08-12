using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.DTOs.User
{
    public class UserResponse
    {
        public Guid UserId { get; set; } = Guid.NewGuid();

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;}

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
