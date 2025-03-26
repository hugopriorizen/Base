using Application.Identity.DTOs;
using MediatR;

namespace Application.Identity.Queries.GetUserById;

public record GetUserByIdQuery(string UserId) : IRequest<UserDto?>;
