using Microsoft.AspNetCore.Http;

namespace EcommerceAPI.Application.Exceptions;

public class BadRequestException : AppException
{
    public override int StatusCode => StatusCodes.Status400BadRequest;
    public override string Title => "Bad request";

    public BadRequestException(string message) : base(message) { }
}