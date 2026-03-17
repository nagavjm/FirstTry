using FirstTry.Application.Features.Institution.DTOs;

namespace FirstTry.Application.Common.Interfaces;

public interface IInstitutionService
{
    Task<InstitutionResponseDto> CreateInstitutionAsync(CreateInstitutionRequestDto request);
    Task<InstitutionResponseDto> GetInstitutionByIdAsync(Guid id);
    Task<InstitutionResponseDto> GetMyInstitutionAsync(string userId);
    Task<InstitutionListResponseDto> GetAllInstitutionsAsync();
    Task<InstitutionListResponseDto> SearchInstitutionsAsync(string keyword);
    Task<InstitutionResponseDto> UpdateInstitutionAsync(Guid id, UpdateInstitutionRequestDto request);
    Task<InstitutionResponseDto> DeleteInstitutionAsync(Guid id);
    Task<InstitutionResponseDto> AddUserToInstitutionAsync(Guid institutionId, AddUserToInstitutionRequestDto request);
    Task<InstitutionResponseDto> RemoveUserFromInstitutionAsync(Guid institutionId, string userId);
    Task<UserResponseDto> UpdateInstitutionUserAsync(Guid institutionId, string userId, UpdateInstitutionUserRequestDto request);
}

