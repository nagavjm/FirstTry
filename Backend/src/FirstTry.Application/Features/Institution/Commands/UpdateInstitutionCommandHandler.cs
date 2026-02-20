using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.Institution.DTOs;
using MediatR;

namespace FirstTry.Application.Features.Institution.Commands;

public class UpdateInstitutionCommandHandler : IRequestHandler<UpdateInstitutionCommand, InstitutionResponseDto>
{
    private readonly IInstitutionService _institutionService;

    public UpdateInstitutionCommandHandler(IInstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    public async Task<InstitutionResponseDto> Handle(UpdateInstitutionCommand request, CancellationToken cancellationToken)
    {
        var updateDto = new UpdateInstitutionRequestDto
        {
            Name = request.Name,
            UniversalId = request.UniversalId,
            Email = request.Email,
            Description = request.Description,
            Address = request.Address,
            Phone = request.Phone,
            Website = request.Website,
            IsActive = request.IsActive
        };

        return await _institutionService.UpdateInstitutionAsync(request.Id, updateDto);
    }
}

