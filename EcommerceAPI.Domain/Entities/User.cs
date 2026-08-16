using EcommerceAPI.Domain.Enums;

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
        public DateTime BirthDate { get; set; } = DateTime.UtcNow;
        public string HashedPassword { get; set; } = string.Empty;
        public Role Role { get; set; } = Role.Customer;

        public bool IsActive { get; set; }=false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public List<UserAddress> Addresses { get; set; } = new List<UserAddress>();
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public List<UserCategory> PreferredCategories { get; set; } = new List<UserCategory>();
        public List<VerificationToken> VerificationTokens { get; set; } = new List<VerificationToken>();

    }
}
