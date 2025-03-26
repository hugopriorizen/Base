using Application.Identity.DTOs;
using MediatR;

namespace Application.Identity.Commands.LoginUser;

public record LoginUserCommand(LoginDto LoginDto) : IRequest<(bool Succeeded, string Message)>;
