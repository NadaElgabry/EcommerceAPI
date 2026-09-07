public class GetAllOrdersRequest
{
    public string? Cursor { get; set; }
    public int Limit { get; set; } = 10;
    public string? Status { get; set; }
}