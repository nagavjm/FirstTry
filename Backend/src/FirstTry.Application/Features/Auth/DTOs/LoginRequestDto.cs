namespace FirstTry.Application.Features.Auth.DTOs;

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

