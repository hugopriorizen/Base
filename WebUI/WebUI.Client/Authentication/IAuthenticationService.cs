using Application.Identity.DTOs;

namespace WebUI.Client.Authentication;

public interface IAuthenticationService
{
    Task<(bool Success, string Message)> LoginAsync(LoginDto loginDto);
    Task<(bool Success, string Message)> RegisterAsync(RegisterUserDto registerDto);
    Task<(bool Success, string Message)> LogoutAsync();
    Task<(bool Success, string Message)> ResetPasswordAsync(PasswordResetDto resetDto);
    Task<(bool Success, string Message)> ChangePasswordAsync(PasswordChangeDto changeDto);
    Task<(bool Success, string Message)> UpdateProfileAsync(UpdateUserDto updateDto);
    Task<UserDto?> GetCurrentUserAsync();
}
