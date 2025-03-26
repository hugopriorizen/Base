using Domain.Repository.Interface;
using Infra.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infra.Identity.Repository;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<Domain.Models.ApplicationUser> _userManager;

    public RoleRepository(
        ApplicationDbContext context,
        RoleManager<IdentityRole> roleManager,
        UserManager<Domain.Models.ApplicationUser> userManager
    )
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<IdentityRole?> GetByIdAsync(string id)
    {
        return await _roleManager.FindByIdAsync(id);
    }

    public async Task<IdentityRole?> GetByNameAsync(string name)
    {
        return await _roleManager.FindByNameAsync(name);
    }

    public async Task<IEnumerable<IdentityRole>> GetAllAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<bool> ExistsAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        return role != null;
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        var role = await _roleManager.FindByNameAsync(name);
        return role != null;
    }

    public async Task<int> GetRoleCountAsync()
    {
        return await _context.Roles.CountAsync();
    }

    public async Task<IEnumerable<IdentityRole>> GetRolesByUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Enumerable.Empty<IdentityRole>();
        }

        var roleNames = await _userManager.GetRolesAsync(user);
        return await _context.Roles.Where(r => roleNames.Contains(r.Name!)).ToListAsync();
    }

    public async Task<IEnumerable<string>> GetRoleNamesAsync()
    {
        return await _context.Roles.Select(r => r.Name!).ToListAsync();
    }

    public async Task<bool> IsRoleAssignedToUserAsync(string roleName, string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        return await _userManager.IsInRoleAsync(user, roleName);
    }
}
