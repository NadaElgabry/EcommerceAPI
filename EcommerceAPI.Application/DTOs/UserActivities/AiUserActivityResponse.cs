namespace EcommerceAPI.Application.DTOs.UserActivities
{
    public class AiUserActivityResponse
    {
        public Guid UserId { get; set; }
        public int? ProductId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}