using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.ChildApplication.DTOs;
using MediatR;

namespace FirstTry.Application.Features.ChildApplication.Queries;

public class GetUserApplicationsQueryHandler : IRequestHandler<GetUserApplicationsQuery, ChildApplicationListResponseDto>
{
    private readonly IChildApplicationService _childApplicationService;

    public GetUserApplicationsQueryHandler(IChildApplicationService childApplicationService)
    {
        _childApplicationService = childApplicationService;
    }

    public async Task<ChildApplicationListResponseDto> Handle(GetUserApplicationsQuery request, CancellationToken cancellationToken)
    {
        return await _childApplicationService.GetAllApplicationsAsync(request.UserId);
    }
}

