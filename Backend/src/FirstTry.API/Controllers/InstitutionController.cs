using FirstTry.Application.Features.Institution.Commands;
using FirstTry.Application.Features.Institution.DTOs;
using FirstTry.Application.Features.Institution.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FirstTry.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstitutionController : ControllerBase
{
    private readonly IMediator _mediator;

    public InstitutionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<InstitutionResponseDto>> Create([FromBody] CreateInstitutionCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Institution?.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<InstitutionListResponseDto>> GetAll()
    {
        var result = await _mediator.Send(new GetAllInstitutionsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InstitutionResponseDto>> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetInstitutionByIdQuery { Id = id });
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost("{id:guid}/users")]
    public async Task<ActionResult<InstitutionResponseDto>> AddUser(Guid id, [FromBody] AddUserToInstitutionRequestDto request)
    {
        var command = new AddUserToInstitutionCommand
        {
            InstitutionId = id,
            Email = request.Email,
            Password = request.Password,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _mediator.Send(command);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult<InstitutionListResponseDto>> Search([FromQuery] string keyword)
    {
        var result = await _mediator.Send(new SearchInstitutionsQuery { Keyword = keyword ?? string.Empty });
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InstitutionResponseDto>> Update(Guid id, [FromBody] UpdateInstitutionRequestDto request)
    {
        var command = new UpdateInstitutionCommand
        {
            Id = id,
            Name = request.Name,
            UniversalId = request.UniversalId,
            Email = request.Email,
            Description = request.Description,
            Address = request.Address,
            Phone = request.Phone,
            Website = request.Website,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpPut("{institutionId:guid}/users/{userId}")]
    public async Task<ActionResult<UserResponseDto>> UpdateUser(Guid institutionId, string userId, [FromBody] UpdateInstitutionUserRequestDto request)
    {
        var command = new UpdateInstitutionUserCommand
        {
            InstitutionId = institutionId,
            UserId = userId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = request.IsActive,
            AuthenticationType = request.AuthenticationType
        };

        var result = await _mediator.Send(command);
        if (!result.Success)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}

