using FirstTry.Application.Features.Institution.DTOs;
using MediatR;

namespace FirstTry.Application.Features.Institution.Queries;

public class SearchInstitutionsQuery : IRequest<InstitutionListResponseDto>
{
    public string Keyword { get; set; } = string.Empty;
}

