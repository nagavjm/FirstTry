using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.Auth.DTOs;
using MediatR;

namespace FirstTry.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequestDto
        {
            Email = request.Email,
            Password = request.Password,
            RememberMe = request.RememberMe,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent
        };

        return await _authService.LoginAsync(loginRequest, cancellationToken);
    }
}

