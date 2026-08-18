using Microsoft.AspNetCore.Http;

namespace EcommerceAPI.Application.Exceptions;

public class ConflictException : AppException
{
    public override int StatusCode => StatusCodes.Status409Conflict;
    public override string Title => "Conflict";

    public ConflictException(string message) : base(message) { }
}