using EcommerceAPI.Application.DTOs.Auth;
using EcommerceAPI.Application.Interfaces.Iservices;
using FluentValidation;

using AppValidationException =
    EcommerceAPI.Application.Exceptions.ValidationException;

namespace EcommerceAPI.Application.UseCases.Auth
{
    public class RegisterUseCase
    {
        private readonly IValidator<RegisterRequest> _validator;
        private readonly IUserService _userService;

        public RegisterUseCase(
            IValidator<RegisterRequest> validator,
            IUserService userService)
        {
            _validator = validator;
            _userService = userService;
        }

        public async Task<AuthResponse> ExecuteAsync(
            RegisterRequest request,
            string ipAddress,string deviceInfo,
            CancellationToken cancellationToken = default)
        {
            var validationResult =
                await _validator.ValidateAsync(
                    request,
                    cancellationToken
                );

            if (!validationResult.IsValid)
            {
                Dictionary<string, string[]> errors =
                    validationResult.Errors
                        .GroupBy(error => error.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group
                                .Select(error => error.ErrorMessage)
                                .ToArray()
                        );

                throw new AppValidationException(errors);
            }

            return await _userService.CreateUserAsync(
                request,ipAddress,deviceInfo,
                cancellationToken
            );
        }
    }
}