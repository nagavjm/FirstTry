using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.Auth.DTOs;
using FirstTry.Domain.Entities;
using FirstTry.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FirstTry.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Create login record for tracking
        var userLogin = new UserLogin
        {
            Id = Guid.NewGuid(),
            UserId = user?.Id ?? "unknown",
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            LoginTime = DateTime.UtcNow,
            IsSuccessful = false,
            IsActive = false
        };

        if (user == null)
        {
            userLogin.FailureReason = "Invalid email";
            // Don't save login attempt for non-existent users to prevent enumeration
            return new AuthResponseDto { Success = false, Message = "Invalid email or password" };
        }

        userLogin.UserId = user.Id;

        if (!user.IsActive)
        {
            userLogin.FailureReason = "Account deactivated";
            await SaveLoginAttempt(userLogin, cancellationToken);
            return new AuthResponseDto { Success = false, Message = "Account is deactivated" };
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (!result.Succeeded)
        {
            user.FailedLoginAttempts++;
            await _userManager.UpdateAsync(user);

            userLogin.FailureReason = "Invalid password";
            await SaveLoginAttempt(userLogin, cancellationToken);
            return new AuthResponseDto { Success = false, Message = "Invalid email or password" };
        }

        // Successful login
        user.LastLoginAt = DateTime.UtcNow;
        user.LoginCount++;
        user.FailedLoginAttempts = 0;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var token = await GenerateJwtTokenAsync(user.Id, user.Email!, roles);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, request.IpAddress, cancellationToken);

        // Update login record
        userLogin.IsSuccessful = true;
        userLogin.IsActive = true;
        userLogin.Token = token;
        userLogin.TokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes);
        await SaveLoginAttempt(userLogin, cancellationToken);

        return new AuthResponseDto
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken.Token,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Roles = roles,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            LoginSessionId = userLogin.Id,
            Message = "Login successfully"
        };
    }

    private async Task SaveLoginAttempt(UserLogin userLogin, CancellationToken cancellationToken)
    {
        _context.UserLogins.Add(userLogin);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string? ipAddress, CancellationToken cancellationToken)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IpAddress = ipAddress
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return refreshToken;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new AuthResponseDto { Success = false, Message = "Email already registered" };
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new AuthResponseDto { Success = false, Message = errors };
        }

        await _userManager.AddToRoleAsync(user, "User");
        var roles = await _userManager.GetRolesAsync(user);
        var token = await GenerateJwtTokenAsync(user.Id, user.Email!, roles);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, null, cancellationToken);

        return new AuthResponseDto
        {
            Success = true,
            Token = token,
            RefreshToken = refreshToken.Token,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Roles = roles,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            Message = "Registration successful"
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken == null)
        {
            return new AuthResponseDto { Success = false, Message = "Invalid refresh token" };
        }

        if (refreshToken.IsRevoked)
        {
            // Revoke all descendant tokens in case of token reuse attack
            await RevokeDescendantRefreshTokensAsync(refreshToken, request.IpAddress, "Attempted reuse of revoked token", cancellationToken);
            return new AuthResponseDto { Success = false, Message = "Invalid refresh token" };
        }

        if (refreshToken.IsExpired)
        {
            return new AuthResponseDto { Success = false, Message = "Refresh token expired" };
        }

        var user = refreshToken.User;
        if (user == null || !user.IsActive)
        {
            return new AuthResponseDto { Success = false, Message = "User not found or inactive" };
        }

        // Rotate refresh token
        var newRefreshToken = await RotateRefreshTokenAsync(refreshToken, request.IpAddress, cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var newToken = await GenerateJwtTokenAsync(user.Id, user.Email!, roles);

        return new AuthResponseDto
        {
            Success = true,
            Token = newToken,
            RefreshToken = newRefreshToken.Token,
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Roles = roles,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            RefreshTokenExpiresAt = newRefreshToken.ExpiresAt,
            Message = "Token refreshed successfully"
        };
    }

    private async Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken oldToken, string? ipAddress, CancellationToken cancellationToken)
    {
        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = oldToken.UserId,
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IpAddress = ipAddress
        };

        oldToken.RevokedAt = DateTime.UtcNow;
        oldToken.ReplacedByToken = newRefreshToken.Token;
        oldToken.ReasonRevoked = "Rotated";

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return newRefreshToken;
    }

    private async Task RevokeDescendantRefreshTokensAsync(RefreshToken token, string? ipAddress, string reason, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(token.ReplacedByToken))
        {
            var childToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token.ReplacedByToken, cancellationToken);

            if (childToken != null)
            {
                if (childToken.IsActive)
                {
                    childToken.RevokedAt = DateTime.UtcNow;
                    childToken.ReasonRevoked = reason;
                }
                await RevokeDescendantRefreshTokensAsync(childToken, ipAddress, reason, cancellationToken);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> LogoutAsync(string userId, string? token = null, CancellationToken cancellationToken = default)
    {
        // Deactivate all active login sessions for the user
        var activeLogins = await _context.UserLogins
            .Where(ul => ul.UserId == userId && ul.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var login in activeLogins)
        {
            login.IsActive = false;
            login.LogoutTime = DateTime.UtcNow;
        }

        // Revoke all active refresh tokens
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in activeTokens)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.ReasonRevoked = "Logged out";
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RevokeTokenAsync(string token, string? reason = null, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            return false;
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.ReasonRevoked = reason ?? "Manually revoked";
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            LoginCount = user.LoginCount,
            Roles = roles
        };
    }

    public async Task<IEnumerable<LoginHistoryDto>> GetLoginHistoryAsync(string userId, int count = 10, CancellationToken cancellationToken = default)
    {
        var loginHistory = await _context.UserLogins
            .Where(ul => ul.UserId == userId)
            .OrderByDescending(ul => ul.LoginTime)
            .Take(count)
            .Select(ul => new LoginHistoryDto
            {
                Id = ul.Id,
                UserId = ul.UserId,
                IpAddress = ul.IpAddress,
                UserAgent = ul.UserAgent,
                LoginTime = ul.LoginTime,
                LogoutTime = ul.LogoutTime,
                IsSuccessful = ul.IsSuccessful,
                FailureReason = ul.FailureReason,
                IsActive = ul.IsActive
            })
            .ToListAsync(cancellationToken);

        return loginHistory;
    }

    public async Task<AuthResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        // Validate passwords match
        if (request.NewPassword != request.ConfirmPassword)
        {
            return new AuthResponseDto { Success = false, Message = "Passwords do not match" };
        }

        // Find user by email
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return new AuthResponseDto { Success = false, Message = "User not found" };
        }

        if (!user.IsActive)
        {
            return new AuthResponseDto { Success = false, Message = "Account is deactivated" };
        }

        // Remove old password and set new password
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new AuthResponseDto { Success = false, Message = errors };
        }

        // Reset failed login attempts
        user.FailedLoginAttempts = 0;
        await _userManager.UpdateAsync(user);

        return new AuthResponseDto
        {
            Success = true,
            Message = "Password reset successfully"
        };
    }

    public Task<string> GenerateJwtTokenAsync(string userId, string email, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
            signingCredentials: credentials
        );

        return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
    }
}

