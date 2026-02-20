using FirstTry.Application.Features.ChildApplication.DTOs;
using MediatR;

namespace FirstTry.Application.Features.ChildApplication.Queries;

public class GetUserApplicationsQuery : IRequest<ChildApplicationListResponseDto>
{
    public string UserId { get; set; } = string.Empty;
}

