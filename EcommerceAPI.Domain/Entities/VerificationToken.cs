using EcommerceAPI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Domain.Entities
{
    public class VerificationToken
    {
        public int Id { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public VerificationPurpose Purpose { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DateTime? ConsumedAt { get; set; }

        public bool IsActive => ConsumedAt == null && DateTime.UtcNow < ExpiresAt;
    }
}
