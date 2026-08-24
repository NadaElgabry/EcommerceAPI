namespace EcommerceAPI.Application.DTOs.UserActivities
{
    public class UserActivitiesResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public int? ProductId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
