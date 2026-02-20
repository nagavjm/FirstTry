using FirstTry.Domain.Common;

namespace FirstTry.Domain.Entities;

public class Institution : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string UniversalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}

