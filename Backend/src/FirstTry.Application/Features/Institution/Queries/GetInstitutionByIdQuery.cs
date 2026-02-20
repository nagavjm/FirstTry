using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.Institution.DTOs;
using MediatR;

namespace FirstTry.Application.Features.Institution.Queries;

public class GetInstitutionByIdQuery : IRequest<InstitutionResponseDto>
{
    public Guid Id { get; set; }
}

public class GetInstitutionByIdQueryHandler : IRequestHandler<GetInstitutionByIdQuery, InstitutionResponseDto>
{
    private readonly IInstitutionService _institutionService;

    public GetInstitutionByIdQueryHandler(IInstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    public async Task<InstitutionResponseDto> Handle(GetInstitutionByIdQuery request, CancellationToken cancellationToken)
    {
        return await _institutionService.GetInstitutionByIdAsync(request.Id);
    }
}

