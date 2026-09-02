using EcommerceAPI.Domain.Enums;
using EcommerceAPI.Domain.Entities;

namespace EcommerceAPI.Application.DTOs.User
{
    public class UserResponse
    {
        public Guid Guid { get; set; } = Guid.NewGuid();

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public List<FavoriteCategory> PreferredCategories { get; set; }
        public Role Role { get; set; }
        public bool IsActive {  get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
