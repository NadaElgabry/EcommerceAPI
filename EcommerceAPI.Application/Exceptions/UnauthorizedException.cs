using Microsoft.AspNetCore.Http;

namespace EcommerceAPI.Application.Exceptions;

public class UnauthorizedException : AppException
{
    public override int StatusCode => StatusCodes.Status401Unauthorized;
    public override string Title => "Unauthorized";

    public UnauthorizedException(string message) : base(message) { }
}