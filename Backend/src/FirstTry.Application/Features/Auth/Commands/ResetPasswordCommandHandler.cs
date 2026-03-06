using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.Auth.DTOs;
using MediatR;

namespace FirstTry.Application.Features.Auth.Commands;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, AuthResponseDto>
{
    private readonly IAuthService _authService;

    public ResetPasswordCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<AuthResponseDto> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var resetPasswordRequest = new ResetPasswordRequestDto
        {
            Email = request.Email,
            NewPassword = request.NewPassword,
            ConfirmPassword = request.ConfirmPassword
        };

        return await _authService.ResetPasswordAsync(resetPasswordRequest, cancellationToken);
    }
}

