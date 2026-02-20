using FirstTry.Domain.Common;

namespace FirstTry.Domain.Entities;

public class ChildApplication : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string LaunchUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    // Navigation property
    public virtual ICollection<UserApplicationAccess> UserApplicationAccesses { get; set; } = new List<UserApplicationAccess>();
}

