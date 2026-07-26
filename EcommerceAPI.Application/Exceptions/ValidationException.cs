using Microsoft.AspNetCore.Http;

namespace EcommerceAPI.Application.Exceptions
{
    public class ValidationException : AppException
    {
        public override int StatusCode => StatusCodes.Status400BadRequest;
        public override string Title => "Validation failure";

        public List<ErrorDetail> Errors { get; }

        public ValidationException()
            : base("One or more validation failures occurred.")
        {}

        public ValidationException(IDictionary<string, string[]> errorsDict) : this()
        {
            Errors = errorsDict
                .SelectMany(kvp => kvp.Value.Select(msg => new ErrorDetail { Property = kvp.Key, Error = msg }))
                .ToList();
        }
    }
}