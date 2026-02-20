namespace FirstTry.Application.Features.Auth.DTOs;

public class AuthResponseDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? Message { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? FullName { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public IList<string>? Roles { get; set; }
    public Guid? LoginSessionId { get; set; }
}

