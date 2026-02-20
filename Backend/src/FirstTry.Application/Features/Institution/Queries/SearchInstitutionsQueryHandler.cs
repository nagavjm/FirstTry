using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.Institution.DTOs;
using MediatR;

namespace FirstTry.Application.Features.Institution.Queries;

public class SearchInstitutionsQueryHandler : IRequestHandler<SearchInstitutionsQuery, InstitutionListResponseDto>
{
    private readonly IInstitutionService _institutionService;

    public SearchInstitutionsQueryHandler(IInstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    public async Task<InstitutionListResponseDto> Handle(SearchInstitutionsQuery request, CancellationToken cancellationToken)
    {
        return await _institutionService.SearchInstitutionsAsync(request.Keyword);
    }
}

