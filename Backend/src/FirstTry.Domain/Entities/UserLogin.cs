using FirstTry.Domain.Common;

namespace FirstTry.Domain.Entities;

/// <summary>
/// Entity to track user login attempts and sessions
/// </summary>
public class UserLogin : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime? LogoutTime { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public string? Token { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation property
    public virtual ApplicationUser? User { get; set; }
}

