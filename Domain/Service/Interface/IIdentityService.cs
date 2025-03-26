using Domain.Models;

namespace Domain.Service.Interface;

public interface IIdentityService
{
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
    Task<ApplicationUser?> GetUserByUserNameAsync(string userName);
    Task<bool> IsUserInRoleAsync(string userId, string role);
    Task<IList<string>> GetUserRolesAsync(string userId);
    Task<bool> ValidateUserAsync(string userName, string password);
    Task<(bool Success, string? Error)> CreateUserAsync(ApplicationUser user, string password);
    Task<(bool Success, string? Error)> UpdateUserAsync(ApplicationUser user);
    Task<(bool Success, string? Error)> DeleteUserAsync(string userId);
    Task<(bool Success, string? Error)> ChangePasswordAsync(
        string userId,
        string currentPassword,
        string newPassword
    );
    Task<(bool Success, string? Error)> ResetPasswordAsync(
        string userId,
        string token,
        string newPassword
    );
    Task<string> GeneratePasswordResetTokenAsync(string userId);
    Task<bool> ConfirmEmailAsync(string userId, string token);
    Task<string> GenerateEmailConfirmationTokenAsync(string userId);
}
