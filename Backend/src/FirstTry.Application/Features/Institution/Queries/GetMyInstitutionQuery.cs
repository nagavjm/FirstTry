using FirstTry.Application.Features.Institution.DTOs;
using MediatR;

namespace FirstTry.Application.Features.Institution.Queries;

public class GetMyInstitutionQuery : IRequest<InstitutionResponseDto>
{
    public string UserId { get; set; } = string.Empty;
}

