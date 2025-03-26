using System.Net.Http.Json;
using System.Security.Claims;
using Application.Identity.DTOs;
using Microsoft.AspNetCore.Components.Authorization;

namespace WebUI.Client.Authentication;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private UserDto? _cachedUser;

    public CustomAuthenticationStateProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();

        try
        {
            var userInfo = await GetCurrentUserInfo();

            if (userInfo?.UserName != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userInfo.UserName),
                    new Claim(ClaimTypes.NameIdentifier, userInfo.Id),
                    new Claim(ClaimTypes.Email, userInfo.Email),
                };

                // Add role claims
                foreach (var role in userInfo.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                identity = new ClaimsIdentity(claims, "server authentication");
            }
        }
        catch (Exception)
        {
            // If we get an exception, the user is not authenticated
            _cachedUser = null;
        }

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task<UserDto?> GetCurrentUserInfo()
    {
        if (_cachedUser != null)
        {
            return _cachedUser;
        }

        var response = await _httpClient.GetAsync("api/identity/currentuser");

        if (response.IsSuccessStatusCode)
        {
            _cachedUser = await response.Content.ReadFromJsonAsync<UserDto>();
            return _cachedUser;
        }

        return null;
    }

    public void NotifyUserAuthentication(UserDto user)
    {
        _cachedUser = user;
        var identity = CreateIdentityFromUser(user);
        var principal = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    public void NotifyUserLogout()
    {
        _cachedUser = null;
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    private ClaimsIdentity CreateIdentityFromUser(UserDto user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
        };

        // Add role claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsIdentity(claims, "server authentication");
    }
}
