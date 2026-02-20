using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.Institution.DTOs;
using MediatR;

namespace FirstTry.Application.Features.Institution.Queries;

public class GetAllInstitutionsQuery : IRequest<InstitutionListResponseDto>
{
}

public class GetAllInstitutionsQueryHandler : IRequestHandler<GetAllInstitutionsQuery, InstitutionListResponseDto>
{
    private readonly IInstitutionService _institutionService;

    public GetAllInstitutionsQueryHandler(IInstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    public async Task<InstitutionListResponseDto> Handle(GetAllInstitutionsQuery request, CancellationToken cancellationToken)
    {
        return await _institutionService.GetAllInstitutionsAsync();
    }
}

