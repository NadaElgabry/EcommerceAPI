namespace EcommerceAPI.Application.DTOs.UserActivities
{
    public class UserActivitiesResponse
    {
        public Guid UserId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
