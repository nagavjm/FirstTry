using FirstTry.Domain.Common;

namespace FirstTry.Domain.Entities;

public class UserApplicationAccess : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public Guid ChildApplicationId { get; set; }
    public bool HasAccess { get; set; } = true;
    public DateTime? GrantedAt { get; set; }
    public string? GrantedBy { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedBy { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ChildApplication ChildApplication { get; set; } = null!;
}

