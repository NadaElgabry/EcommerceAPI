using EcommerceAPI.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceAPI.Controllers
{
    [ApiController]
    [Route("api/test-exceptions")]
    public class TestExceptionsController : ControllerBase
    {
        // 400 - Bad Request
        [HttpGet("bad-request")]
        public IActionResult ThrowBadRequest()
        {
            throw new BadRequestException("The request is malformed.");
        }

        // 400 - Validation (with error list, using dictionary ctor)
        [HttpGet("validation")]
        public IActionResult ThrowValidation()
        {
            var errors = new Dictionary<string, string[]>
            {
                { "Email", new[] { "Email is required.", "Email format is invalid." } },
                { "Password", new[] { "Password must be at least 8 characters." } }
            };
            throw new ValidationException(errors);
        }

        // 403 - Forbidden
        [HttpGet("forbidden")]
        public IActionResult ThrowForbidden()
        {
            throw new ForbiddenException("You do not have permission to access this resource.");
        }

        // 401 - Unauthorized (BCL exception, hits your switch case)
        [HttpGet("unauthorized")]
        public IActionResult ThrowUnauthorized()
        {
            throw new UnauthorizedAccessException("Access token is missing or invalid.");
        }

        // 404 - Not Found (BCL exception, hits your switch case)
        [HttpGet("not-found")]
        public IActionResult ThrowNotFound()
        {
            throw new KeyNotFoundException("Order with ID 12345 was not found.");
        }

        // 499 - Cancelled (harder to trigger naturally, but this forces it)
        [HttpGet("cancelled")]
        public IActionResult ThrowCancelled()
        {
            throw new OperationCanceledException("The operation was cancelled.");
        }

        // 500 - Unexpected/unhandled exception (falls to default case)
        [HttpGet("server-error")]
        public IActionResult ThrowServerError()
        {
            throw new InvalidOperationException("Something went wrong internally.");
        }

        // 500 - Null reference, another common "unexpected" case
        [HttpGet("null-reference")]
        public IActionResult ThrowNullReference()
        {
            string? value = null;
            return Ok(value!.Length); // throws NullReferenceException
        }

        // Divide by zero - another good default-case test
        [HttpGet("divide-by-zero")]
        public IActionResult ThrowDivideByZero()
        {
            int zero = 0;
            var result = 10 / zero;
            return Ok(result);
        }

        // Sanity check - confirms the endpoint/pipeline itself works
        [HttpGet("ok")]
        public IActionResult Success()
        {
            return Ok(new { message = "No exception thrown." });
        }
    }
}