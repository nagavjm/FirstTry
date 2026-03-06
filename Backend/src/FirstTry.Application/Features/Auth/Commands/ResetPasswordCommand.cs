using FirstTry.Application.Features.Auth.DTOs;
using MediatR;

namespace FirstTry.Application.Features.Auth.Commands;

public class ResetPasswordCommand : IRequest<AuthResponseDto>
{
    public string Email { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

