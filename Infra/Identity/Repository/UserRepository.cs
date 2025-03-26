using Domain.Models;
using Domain.Repository.Interface;
using Infra.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infra.Identity.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<ApplicationUser?> GetByUserNameAsync(string userName)
    {
        return await _userManager.FindByNameAsync(userName);
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
    {
        return await _context.Users.Where(u => u.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetByRoleAsync(string role)
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync(role);
        return usersInRole.Where(u => u.IsActive);
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email && u.IsActive);
    }

    public async Task<bool> ExistsByUserNameAsync(string userName)
    {
        return await _context.Users.AnyAsync(u => u.UserName == userName && u.IsActive);
    }

    public async Task<int> GetUserCountAsync()
    {
        return await _context.Users.CountAsync(u => u.IsActive);
    }

    public async Task<IEnumerable<ApplicationUser>> GetInactiveUsersAsync()
    {
        return await _context.Users.Where(u => !u.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> GetUsersCreatedAfterAsync(DateTimeOffset date)
    {
        return await _context.Users.Where(u => u.IsActive && u.CreatedAt >= date).ToListAsync();
    }
}
