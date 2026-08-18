namespace EcommerceAPI.Domain.Entities
{
    public class UserAddress
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string Location { get; set; } = string.Empty;

    }
}
