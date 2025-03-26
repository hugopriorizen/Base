using Domain.Models;
using Domain.Repository.Interface;
using Domain.Service.Interface;
using Microsoft.AspNetCore.Identity;

namespace Infra.Identity.Service;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUserRepository userRepository,
        IRoleRepository roleRepository
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<ApplicationUser?> GetUserByUserNameAsync(string userName)
    {
        return await _userRepository.GetByUserNameAsync(userName);
    }

    public async Task<bool> IsUserInRoleAsync(string userId, string role)
    {
        return await _roleRepository.IsRoleAssignedToUserAsync(role, userId);
    }

    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new List<string>();
        }

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> ValidateUserAsync(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null || !user.IsActive)
        {
            return false;
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);

        if (result.Succeeded)
        {
            // Update LastLoginAt
            user.LastLoginAt = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        return result.Succeeded;
    }

    public async Task<(bool Success, string? Error)> CreateUserAsync(
        ApplicationUser user,
        string password
    )
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateUserAsync(ApplicationUser user)
    {
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found");
        }

        // Implement soft delete
        user.IsActive = false;
        user.DeletedAt = DateTimeOffset.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword
    )
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found");
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(
        string userId,
        string token,
        string newPassword
    )
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, "User not found");
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        return (true, null);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found", nameof(userId));
        }

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            throw new ArgumentException("User not found", nameof(userId));
        }

        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }
}
