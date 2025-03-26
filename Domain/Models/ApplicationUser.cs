using System.ComponentModel.DataAnnotations;
using Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Domain.Models;

public class ApplicationUser : IdentityUser, IBaseEntity
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Address { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? DeletedAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}".Trim();
}
