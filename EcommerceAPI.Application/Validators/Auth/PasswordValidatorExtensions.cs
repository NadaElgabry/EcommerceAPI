using FluentValidation;

namespace EcommerceAPI.Application.Validators.Auth
{
    public static class PasswordValidatorExtensions
    {
        public static IRuleBuilderOptions<T, string> ValidPassword<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            int minLength = 8,
            int maxLength = 100)
        {
            return ruleBuilder
                .NotEmpty()
                    .WithMessage("Password is required.")
                .MinimumLength(minLength)
                    .WithMessage($"Password must be at least {minLength} characters long.")
                .MaximumLength(maxLength)
                    .WithMessage($"Password cannot exceed {maxLength} characters.")
                .Must(HaveRequiredCharacterTypes)
                    .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character.")
                .Must(NotContainWhitespace)
                    .WithMessage("Password cannot contain spaces.");
        }

        private static bool HaveRequiredCharacterTypes(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpper = true;
                else if (char.IsLower(c)) hasLower = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (!char.IsWhiteSpace(c)) hasSpecial = true;

                if (hasUpper && hasLower && hasDigit && hasSpecial)
                    return true;
            }

            return false;
        }

        private static bool NotContainWhitespace(string password)
        {
            foreach (char c in password)
            {
                if (char.IsWhiteSpace(c)) return false;
            }
            return true;
        }
    }
}