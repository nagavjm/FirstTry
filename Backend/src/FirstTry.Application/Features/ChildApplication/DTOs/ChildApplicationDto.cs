namespace FirstTry.Application.Features.ChildApplication.DTOs;

public class ChildApplicationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string LaunchUrl { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool HasAccess { get; set; }
}

public class ChildApplicationListResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public List<ChildApplicationDto> Applications { get; set; } = new();
}

