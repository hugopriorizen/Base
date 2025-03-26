using Domain.Service.Interface;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Identity.Commands.LoginUser;

public class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, (bool Succeeded, string Message)>
{
    private readonly IIdentityService _identityService;
    private readonly IValidator<DTOs.LoginDto> _validator;
    private readonly ILogger<LoginUserCommandHandler> _logger;

    public LoginUserCommandHandler(
        IIdentityService identityService,
        IValidator<DTOs.LoginDto> validator,
        ILogger<LoginUserCommandHandler> logger
    )
    {
        _identityService = identityService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<(bool Succeeded, string Message)> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            // Validate the request
            var validationResult = await _validator.ValidateAsync(
                request.LoginDto,
                cancellationToken
            );
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return (false, errors);
            }

            // Validate the user credentials
            var isValid = await _identityService.ValidateUserAsync(
                request.LoginDto.UserName,
                request.LoginDto.Password
            );

            if (!isValid)
            {
                return (false, "Invalid username or password");
            }

            _logger.LogInformation(
                "User {Username} logged in successfully",
                request.LoginDto.UserName
            );
            return (true, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred during login for user {Username}",
                request.LoginDto.UserName
            );
            return (false, $"An error occurred: {ex.Message}");
        }
    }
}
