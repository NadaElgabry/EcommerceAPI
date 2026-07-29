using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using System.Text;

namespace EcommerceAPI.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public List<UserAddress> Addresses { get; set; } = new List<UserAddress>();
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    }
}
