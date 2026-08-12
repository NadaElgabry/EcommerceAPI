using System;
using System.Collections.Generic;
using System.Text;

namespace EcommerceAPI.Application.Interfaces.Auth
{
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the unique identifier of the currently authenticated user.
        /// </summary>
        public Guid UserGuid { get; }

        /// <summary>
        /// Gets the role of the currently authenticated user.
        /// </summary>
        public string? Role { get; }

        /// <summary>
        /// Checks whether the user is authenticated or not.
        /// </summary>
        public bool IsAuthenticated { get; }
    }
}
