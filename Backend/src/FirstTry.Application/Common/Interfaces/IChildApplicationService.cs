using FirstTry.Application.Features.ChildApplication.DTOs;

namespace FirstTry.Application.Common.Interfaces;

public interface IChildApplicationService
{
    Task<ChildApplicationListResponseDto> GetAllApplicationsAsync(string userId);
    Task<ChildApplicationListResponseDto> GetAccessibleApplicationsAsync(string userId);
}

