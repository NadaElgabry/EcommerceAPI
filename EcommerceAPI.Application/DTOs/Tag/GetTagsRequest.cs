namespace EcommerceAPI.Application.DTOs.Tag
{
    public class GetTagsRequest
    {
        public int PageNumber { get; set; } = 0;
        public int PageSize { get; set; } = 10;
    }
}
