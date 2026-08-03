namespace EcommerceAPI.Application.Interfaces.Auth
{
    public interface IPasswordHasher
    {
        /// <summary>
        /// Hashes the given password and returns the hashed value.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <returns>The hashed password.</returns>
        public string Hash(string password);

        /// <summary>
        /// Verifies the given password against the hashed password.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="hashedPassword">The hashed password to compare against.</param>
        /// <returns>true if the passwords match; otherwise, false.</returns>
        public bool Verify(string password, string hashedPassword);

    }
}
