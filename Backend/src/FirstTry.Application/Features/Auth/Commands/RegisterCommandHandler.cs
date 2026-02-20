using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.Auth.DTOs;
using MediatR;

namespace FirstTry.Application.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var registerRequest = new RegisterRequestDto
        {
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        return await _authService.RegisterAsync(registerRequest, cancellationToken);
    }
}

