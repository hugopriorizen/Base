using Domain.Models;

namespace Domain.Repository.Interface;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string id);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<ApplicationUser?> GetByUserNameAsync(string userName);
    Task<IEnumerable<ApplicationUser>> GetAllAsync();
    Task<IEnumerable<ApplicationUser>> GetByRoleAsync(string role);
    Task<bool> ExistsAsync(string id);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUserNameAsync(string userName);
    Task<int> GetUserCountAsync();
    Task<IEnumerable<ApplicationUser>> GetInactiveUsersAsync();
    Task<IEnumerable<ApplicationUser>> GetUsersCreatedAfterAsync(DateTimeOffset date);
}
