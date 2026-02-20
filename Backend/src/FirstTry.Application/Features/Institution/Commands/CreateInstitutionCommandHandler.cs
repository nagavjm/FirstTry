using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.Institution.DTOs;
using MediatR;

namespace FirstTry.Application.Features.Institution.Commands;

public class CreateInstitutionCommandHandler : IRequestHandler<CreateInstitutionCommand, InstitutionResponseDto>
{
    private readonly IInstitutionService _institutionService;

    public CreateInstitutionCommandHandler(IInstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    public async Task<InstitutionResponseDto> Handle(CreateInstitutionCommand request, CancellationToken cancellationToken)
    {
        var createRequest = new CreateInstitutionRequestDto
        {
            Name = request.Name,
            UniversalId = request.UniversalId,
            Email = request.Email,
            Description = request.Description,
            Address = request.Address,
            Phone = request.Phone,
            Website = request.Website,
            Users = request.Users
        };

        return await _institutionService.CreateInstitutionAsync(createRequest);
    }
}

