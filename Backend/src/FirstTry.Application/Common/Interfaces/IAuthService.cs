using FirstTry.Application.Features.Auth.DTOs;

namespace FirstTry.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(string userId, string? token = null, CancellationToken cancellationToken = default);
    Task<bool> RevokeTokenAsync(string token, string? reason = null, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<LoginHistoryDto>> GetLoginHistoryAsync(string userId, int count = 10, CancellationToken cancellationToken = default);
    Task<string> GenerateJwtTokenAsync(string userId, string email, IList<string> roles);
}

