using Application.Identity.DTOs;
using AutoMapper;
using Domain.Service.Interface;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Identity.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        IIdentityService identityService,
        IMapper mapper,
        ILogger<GetUserByIdQueryHandler> logger
    )
    {
        _identityService = identityService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserDto?> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var user = await _identityService.GetUserByIdAsync(request.UserId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", request.UserId);
                return null;
            }

            var userDto = _mapper.Map<UserDto>(user);

            // Get user roles
            var roles = await _identityService.GetUserRolesAsync(request.UserId);
            userDto.Roles = roles;

            return userDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error occurred while getting user with ID {UserId}",
                request.UserId
            );
            return null;
        }
    }
}
