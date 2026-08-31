namespace EcommerceAPI.Application.DTOs.Order
{
    public class GetOrdersRequest
    {
        public string? Cursor { get; set; }

        public int Limit { get; set; } = 10;
    }
}