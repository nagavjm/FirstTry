using FirstTry.Domain.Common;
using FirstTry.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstTry.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public new DbSet<UserLogin> UserLogins { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Institution> Institutions { get; set; }
    public DbSet<ChildApplication> ChildApplications { get; set; }
    public DbSet<UserApplicationAccess> UserApplicationAccesses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Institution
        builder.Entity<Institution>(entity =>
        {
            entity.ToTable("Institutions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UniversalId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Website).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasIndex(e => e.UniversalId).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LoginCount).HasDefaultValue(0);
            entity.Property(e => e.FailedLoginAttempts).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            // Institution relationship
            entity.HasOne(e => e.Institution)
                .WithMany(i => i.Users)
                .HasForeignKey(e => e.InstitutionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.InstitutionId);

            // Ignore computed property
            entity.Ignore(e => e.FullName);
        });

        // Configure UserLogin
        builder.Entity<UserLogin>(entity =>
        {
            entity.ToTable("UserLogins");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.Token).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserLogins)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.LoginTime);
        });

        // Configure RefreshToken
        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(500);
            entity.Property(e => e.ReasonRevoked).HasMaxLength(200);
            entity.Property(e => e.IpAddress).HasMaxLength(50);

            // Ignore computed properties
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsRevoked);
            entity.Ignore(e => e.IsActive);

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.Token);
            entity.HasIndex(e => e.UserId);
        });

        // Configure ChildApplication
        builder.Entity<ChildApplication>(entity =>
        {
            entity.ToTable("ChildApplications");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IconUrl).HasMaxLength(500);
            entity.Property(e => e.LaunchUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure UserApplicationAccess
        builder.Entity<UserApplicationAccess>(entity =>
        {
            entity.ToTable("UserApplicationAccesses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.HasAccess).HasDefaultValue(true);
            entity.Property(e => e.GrantedBy).HasMaxLength(450);
            entity.Property(e => e.RevokedBy).HasMaxLength(450);

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserApplicationAccesses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ChildApplication)
                .WithMany(a => a.UserApplicationAccesses)
                .HasForeignKey(e => e.ChildApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.ChildApplicationId }).IsUnique();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<ApplicationUser>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
        }

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}

