using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Exceptions
{
    public class ExternalServiceException : AppException
    {
        public override int StatusCode => StatusCodes.Status502BadGateway;
        public override string Title => "External Service";

        public ExternalServiceException(string message) : base(message) { }
    }
}
