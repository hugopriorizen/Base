using Application.Identity.DTOs;
using MediatR;

namespace Application.Identity.Commands.RegisterUser;

public record RegisterUserCommand(RegisterUserDto UserDto)
    : IRequest<(bool Succeeded, string Message)>;
