using Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infra.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customize the ASP.NET Identity model and override the defaults
        // For example, you can rename the ASP.NET Identity table names

        // builder.Entity<ApplicationUser>().ToTable("Users");
        // builder.Entity<IdentityRole>().ToTable("Roles");
        // builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        // builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        // builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        // builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        // builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");

        // Configure custom entities and relationships here
    }

    // Add any additional DbSets needed for your application below
    // public DbSet<YourEntity> YourEntities { get; set; }

    public override int SaveChanges()
    {
        UpdateSoftDeleteStatuses();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateSoftDeleteStatuses();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateSoftDeleteStatuses()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Domain.Common.IBaseEntity baseEntity)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        baseEntity.CreatedAt = DateTimeOffset.UtcNow;
                        baseEntity.IsActive = true;
                        break;
                    case EntityState.Modified:
                        // If the entity is marked as not active and DeletedAt is null, set it to now
                        if (!baseEntity.IsActive && baseEntity.DeletedAt == null)
                        {
                            baseEntity.DeletedAt = DateTimeOffset.UtcNow;
                        }
                        break;
                    case EntityState.Deleted:
                        // Convert hard delete to soft delete
                        entry.State = EntityState.Modified;
                        baseEntity.IsActive = false;
                        baseEntity.DeletedAt = DateTimeOffset.UtcNow;
                        break;
                }
            }
        }
    }
}
