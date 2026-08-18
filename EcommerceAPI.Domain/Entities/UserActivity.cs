using EcommerceAPI.Domain.Enums;

namespace EcommerceAPI.Domain.Entities
{
    public class UserActivity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? ProductId { get; set; }
        public Product? Product { get; set; }
        public UserActionType ActionType { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
