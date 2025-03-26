using Microsoft.AspNetCore.Identity;

namespace Domain.Repository.Interface;

public interface IRoleRepository
{
    Task<IdentityRole?> GetByIdAsync(string id);
    Task<IdentityRole?> GetByNameAsync(string name);
    Task<IEnumerable<IdentityRole>> GetAllAsync();
    Task<bool> ExistsAsync(string id);
    Task<bool> ExistsByNameAsync(string name);
    Task<int> GetRoleCountAsync();
    Task<IEnumerable<IdentityRole>> GetRolesByUserAsync(string userId);
    Task<IEnumerable<string>> GetRoleNamesAsync();
    Task<bool> IsRoleAssignedToUserAsync(string roleName, string userId);
}
