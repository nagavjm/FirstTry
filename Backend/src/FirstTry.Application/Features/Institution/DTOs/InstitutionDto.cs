using FirstTry.Domain.Enums;

namespace FirstTry.Application.Features.Institution.DTOs;

public class InstitutionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UniversalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
    public List<InstitutionUserDto> Users { get; set; } = new();
}

public class InstitutionUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public AuthenticationType AuthenticationType { get; set; } = AuthenticationType.BasicAuth;
}

public class CreateInstitutionRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string UniversalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    
    // Users to create with the institution (at least one required)
    public List<CreateInstitutionUserDto> Users { get; set; } = new();
}

public class CreateInstitutionUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class UpdateInstitutionRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string UniversalId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; }
}

public class AddUserToInstitutionRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class InstitutionResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public InstitutionDto? Institution { get; set; }
}

public class InstitutionListResponseDto
{
    public bool Success { get; set; }
    public List<InstitutionDto> Institutions { get; set; } = new();
    public int TotalCount { get; set; }
}

public class SearchInstitutionsRequestDto
{
    public string Keyword { get; set; } = string.Empty;
}

public class UpdateInstitutionUserRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public AuthenticationType AuthenticationType { get; set; }
}

public class UserResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public InstitutionUserDto? User { get; set; }
}

