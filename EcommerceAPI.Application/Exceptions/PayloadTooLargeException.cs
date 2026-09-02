using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Exceptions
{
    public class PayloadTooLargeException : AppException
    {
        public override int StatusCode => StatusCodes.Status413PayloadTooLarge;
        public override string Title => "Payload Too Large";

        public PayloadTooLargeException(string message) : base(message) { }
    }
}
