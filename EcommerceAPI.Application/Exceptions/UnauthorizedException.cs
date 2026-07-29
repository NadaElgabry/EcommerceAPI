using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Exceptions
{
    public class UnauthorizedException : AppException
    {
        public override int StatusCode => StatusCodes.Status401Unauthorized;
        public override string Title => "Unauthorized";

        public UnauthorizedException(string message) : base(message) { }
    }
}
