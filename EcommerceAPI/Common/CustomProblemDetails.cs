using EcommerceAPI.Application.Exceptions;

namespace EcommerceAPI.Common
{
    public class CustomProblemDetails
    {
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public string Detail { get; set; } = string.Empty;
        public string Instance { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public List<ErrorDetail>? Errors { get; set; }
    }
}
