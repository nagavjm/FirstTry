using FirstTry.Application.Features.Institution.DTOs;
using MediatR;

namespace FirstTry.Application.Features.Institution.Commands;

public class CreateInstitutionCommand : IRequest<InstitutionResponseDto>
{
    public string Name { get; set; } = string.Empty;
    public string UniversalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public List<CreateInstitutionUserDto> Users { get; set; } = new();
}

