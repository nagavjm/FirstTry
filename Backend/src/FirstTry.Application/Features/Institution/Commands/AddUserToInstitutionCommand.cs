using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.Institution.DTOs;
using MediatR;

namespace FirstTry.Application.Features.Institution.Commands;

public class AddUserToInstitutionCommand : IRequest<InstitutionResponseDto>
{
    public Guid InstitutionId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class AddUserToInstitutionCommandHandler : IRequestHandler<AddUserToInstitutionCommand, InstitutionResponseDto>
{
    private readonly IInstitutionService _institutionService;

    public AddUserToInstitutionCommandHandler(IInstitutionService institutionService)
    {
        _institutionService = institutionService;
    }

    public async Task<InstitutionResponseDto> Handle(AddUserToInstitutionCommand request, CancellationToken cancellationToken)
    {
        var addUserRequest = new AddUserToInstitutionRequestDto
        {
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        return await _institutionService.AddUserToInstitutionAsync(request.InstitutionId, addUserRequest);
    }
}

