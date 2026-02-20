using FirstTry.Domain.Entities;
using FirstTry.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FirstTry.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DatabaseSeeder(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        // Seed roles
        string[] roles = { "Admin", "User", "PenguinAdmin" };
        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed Penguin Institution and Users
        await SeedPenguinInstitutionAsync();

        // Seed Child Applications
        await SeedChildApplicationsAsync();
    }

    private async Task SeedPenguinInstitutionAsync()
    {
        // Check if Penguin institution already exists
        var existingInstitution = await _context.Institutions
            .FirstOrDefaultAsync(i => i.UniversalId == "PENGUIN");

        if (existingInstitution != null)
        {
            return; // Already seeded
        }

        // Create Penguin institution
        var penguinInstitution = new Institution
        {
            Id = Guid.NewGuid(),
            Name = "Penguin Support",
            UniversalId = "PENGUIN",
            Email = "support@penguin.com",
            Description = "Penguin Support Institution - Admin users for supporting other institutions",
            IsActive = true
        };

        _context.Institutions.Add(penguinInstitution);
        await _context.SaveChangesAsync();

        // Create Penguin admin users
        var penguinUsers = new[]
        {
            new { Email = "admin1@penguin.com", FirstName = "Penguin", LastName = "Admin1", Password = "Penguin@123" },
            new { Email = "admin2@penguin.com", FirstName = "Penguin", LastName = "Admin2", Password = "Penguin@123" },
            new { Email = "support@penguin.com", FirstName = "Support", LastName = "Team", Password = "Support@123" }
        };

        foreach (var penguinUser in penguinUsers)
        {
            var existingUser = await _userManager.FindByEmailAsync(penguinUser.Email);
            if (existingUser != null)
            {
                continue;
            }

            var user = new ApplicationUser
            {
                UserName = penguinUser.Email,
                Email = penguinUser.Email,
                FirstName = penguinUser.FirstName,
                LastName = penguinUser.LastName,
                InstitutionId = penguinInstitution.Id,
                IsActive = true,
                EmailConfirmed = true,
                AuthenticationType = AuthenticationType.BasicAuth
            };

            var result = await _userManager.CreateAsync(user, penguinUser.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "PenguinAdmin");
            }
        }
    }

    private async Task SeedChildApplicationsAsync()
    {
        // Check if child apps already exist
        if (await _context.ChildApplications.AnyAsync())
        {
            return; // Already seeded
        }

        var childApps = new[]
        {
            new ChildApplication
            {
                Id = Guid.NewGuid(),
                Name = "Dashboard",
                Description = "Main dashboard application",
                IconUrl = "/assets/icons/dashboard.svg",
                LaunchUrl = "/dashboard",
                DisplayOrder = 1,
                IsActive = true
            },
            new ChildApplication
            {
                Id = Guid.NewGuid(),
                Name = "Reports",
                Description = "Reports and analytics application",
                IconUrl = "/assets/icons/reports.svg",
                LaunchUrl = "/reports",
                DisplayOrder = 2,
                IsActive = true
            },
            new ChildApplication
            {
                Id = Guid.NewGuid(),
                Name = "Settings",
                Description = "System settings and configuration",
                IconUrl = "/assets/icons/settings.svg",
                LaunchUrl = "/settings",
                DisplayOrder = 3,
                IsActive = true
            }
        };

        _context.ChildApplications.AddRange(childApps);
        await _context.SaveChangesAsync();
    }
}

