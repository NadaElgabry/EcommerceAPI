using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public Guid Guid { get; set; } = Guid.NewGuid();

        public string TokenHash { get; set; } = string.Empty;

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }

        public Guid? ReplacedByTokenGuid { get; set; }

        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }

        public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
    }
}
