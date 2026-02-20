using Microsoft.AspNetCore.Identity;
using FirstTry.Domain.Enums;

namespace FirstTry.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int LoginCount { get; set; } = 0;
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEndDate { get; set; }

    // Authentication type (SSO or Basic Auth)
    public AuthenticationType AuthenticationType { get; set; } = AuthenticationType.BasicAuth;

    // Institution relationship
    public Guid? InstitutionId { get; set; }
    public virtual Institution? Institution { get; set; }

    // Navigation properties
    public virtual ICollection<UserLogin> UserLogins { get; set; } = new List<UserLogin>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual ICollection<UserApplicationAccess> UserApplicationAccesses { get; set; } = new List<UserApplicationAccess>();

    // Computed property
    public string FullName => $"{FirstName} {LastName}";
}

