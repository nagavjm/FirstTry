using FirstTry.Application.Features.Institution.DTOs;
using FirstTry.Domain.Enums;
using MediatR;

namespace FirstTry.Application.Features.Institution.Commands;

public class UpdateInstitutionUserCommand : IRequest<UserResponseDto>
{
    public Guid InstitutionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public AuthenticationType AuthenticationType { get; set; }
}

