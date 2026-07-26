using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Exceptions
{
    public abstract class AppException: Exception
    {
        public abstract int StatusCode { get; }
        public abstract string Title { get; }

        protected AppException(string message) : base(message) { }
    } 
}
