using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Exceptions
{
    public class InsufficientStockException : AppException
    {
        public override int StatusCode => StatusCodes.Status409Conflict;
        public override string Title => "InsufficientStock";

        public InsufficientStockException(string message) : base(message) { }
    }
}
