using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.Institution.DTOs;
using FirstTry.Application.Features.Institution.Queries;
using MediatR;

namespace FirstTry.Application.Features.Institution.Handlers;

public class GetMyInstitutionQueryHandler : IRequestHandler<GetMyInstitutionQuery, InstitutionResponseDto>
{
    private readonly IInstitutionService _institutionService;

    public GetMyInstitutionQueryHandler(IInstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    public async Task<InstitutionResponseDto> Handle(GetMyInstitutionQuery request, CancellationToken cancellationToken)
    {
        return await _institutionService.GetMyInstitutionAsync(request.UserId);
    }
}

