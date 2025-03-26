using System.Net.Http.Json;
using Application.Identity.DTOs;

namespace WebUI.Client.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthenticationStateProvider _authStateProvider;

    public AuthenticationService(
        HttpClient httpClient,
        CustomAuthenticationStateProvider authStateProvider
    )
    {
        _httpClient = httpClient;
        _authStateProvider = authStateProvider;
    }

    public async Task<(bool Success, string Message)> LoginAsync(LoginDto loginDto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/identity/login", loginDto);

        if (response.IsSuccessStatusCode)
        {
            var userInfo = await _authStateProvider.GetCurrentUserInfo();
            if (userInfo != null)
            {
                _authStateProvider.NotifyUserAuthentication(userInfo);
                return (true, "Login successful");
            }
        }

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, errorResponse?.Message ?? "Login failed");
    }

    public async Task<(bool Success, string Message)> RegisterAsync(RegisterUserDto registerDto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/identity/register", registerDto);

        if (response.IsSuccessStatusCode)
        {
            return (true, "Registration successful. You can now login.");
        }

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, errorResponse?.Message ?? "Registration failed");
    }

    public async Task<(bool Success, string Message)> LogoutAsync()
    {
        var response = await _httpClient.PostAsync("api/identity/logout", null);

        if (response.IsSuccessStatusCode)
        {
            _authStateProvider.NotifyUserLogout();
            return (true, "Logout successful");
        }

        return (false, "Logout failed");
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(PasswordResetDto resetDto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/identity/resetpassword", resetDto);

        if (response.IsSuccessStatusCode)
        {
            return (true, "Password reset successful");
        }

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, errorResponse?.Message ?? "Password reset failed");
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(
        PasswordChangeDto changeDto
    )
    {
        var response = await _httpClient.PostAsJsonAsync("api/identity/changepassword", changeDto);

        if (response.IsSuccessStatusCode)
        {
            return (true, "Password changed successfully");
        }

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, errorResponse?.Message ?? "Password change failed");
    }

    public async Task<(bool Success, string Message)> UpdateProfileAsync(UpdateUserDto updateDto)
    {
        var response = await _httpClient.PutAsJsonAsync("api/identity/updateprofile", updateDto);

        if (response.IsSuccessStatusCode)
        {
            var userInfo = await _authStateProvider.GetCurrentUserInfo();
            if (userInfo != null)
            {
                _authStateProvider.NotifyUserAuthentication(userInfo);
                return (true, "Profile updated successfully");
            }
        }

        var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, errorResponse?.Message ?? "Profile update failed");
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        return await _authStateProvider.GetCurrentUserInfo();
    }

    private class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
