using AutoMapper;
using Domain.Models;
using Domain.Service.Interface;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Identity.Commands.RegisterUser;

public class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, (bool Succeeded, string Message)>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly IValidator<DTOs.RegisterUserDto> _validator;
    private readonly ILogger<RegisterUserCommandHandler> _logger;

    public RegisterUserCommandHandler(
        IIdentityService identityService,
        IMapper mapper,
        IValidator<DTOs.RegisterUserDto> validator,
        ILogger<RegisterUserCommandHandler> logger
    )
    {
        _identityService = identityService;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<(bool Succeeded, string Message)> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            // Validate the request
            var validationResult = await _validator.ValidateAsync(
                request.UserDto,
                cancellationToken
            );
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return (false, errors);
            }

            // Check if the password and confirmation match
            if (request.UserDto.Password != request.UserDto.ConfirmPassword)
            {
                return (false, "Password and confirmation do not match");
            }

            // Map the DTO to the domain model
            var user = _mapper.Map<ApplicationUser>(request.UserDto);

            // Create the user
            var (success, error) = await _identityService.CreateUserAsync(
                user,
                request.UserDto.Password
            );
            if (!success)
            {
                return (false, error ?? "Failed to create user");
            }

            _logger.LogInformation("User {Username} registered successfully", user.UserName);
            return (true, "User registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while registering user {Username}",
                request.UserDto.UserName
            );
            return (false, $"An error occurred: {ex.Message}");
        }
    }
}
