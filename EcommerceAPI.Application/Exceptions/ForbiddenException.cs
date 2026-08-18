using Microsoft.AspNetCore.Http;

namespace EcommerceAPI.Application.Exceptions;

public class ForbiddenException : AppException
{
    public override int StatusCode => StatusCodes.Status403Forbidden;
    public override string Title => "Forbidden";

    public ForbiddenException(string message) : base(message) { }
}