using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.Institution.DTOs;
using MediatR;

namespace FirstTry.Application.Features.Institution.Commands;

public class UpdateInstitutionUserCommandHandler : IRequestHandler<UpdateInstitutionUserCommand, UserResponseDto>
{
    private readonly IInstitutionService _institutionService;

    public UpdateInstitutionUserCommandHandler(IInstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    public async Task<UserResponseDto> Handle(UpdateInstitutionUserCommand request, CancellationToken cancellationToken)
    {
        var updateDto = new UpdateInstitutionUserRequestDto
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = request.IsActive,
            AuthenticationType = request.AuthenticationType
        };

        return await _institutionService.UpdateInstitutionUserAsync(request.InstitutionId, request.UserId, updateDto);
    }
}

