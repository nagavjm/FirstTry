using FirstTry.Application.Features.ChildApplication.DTOs;
using FirstTry.Application.Features.ChildApplication.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FirstTry.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ChildApplicationListResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ChildApplicationListResponseDto>> GetUserApplications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new ChildApplicationListResponseDto
            {
                Success = false,
                Message = "User not found"
            });
        }

        var result = await _mediator.Send(new GetUserApplicationsQuery { UserId = userId });
        return Ok(result);
    }
}

