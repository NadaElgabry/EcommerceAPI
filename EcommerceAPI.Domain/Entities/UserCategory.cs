namespace EcommerceAPI.Domain.Entities
{
    public class UserCategory
    {
        public int UserId { get; set; }

        public int CategoryId { get; set; }

        public User User { get; set; } = null!;

        public Category Category { get; set; } = null!;
    }
}