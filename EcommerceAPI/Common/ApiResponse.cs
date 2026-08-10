namespace EcommerceAPI.Application.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; } = true;
        public int StatusCode { get; set; } = 200;
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static ApiResponse<T> SuccessResponse(T data, string? message = null, int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = statusCode,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> SuccessResponse(string? message = null, int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = statusCode,
                Data = default,
                Message = message
            };
        }
    }
}