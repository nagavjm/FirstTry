namespace FirstTry.Application.Features.Auth.DTOs;

public class RefreshTokenRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
}

