using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.Institution.DTOs;
using FirstTry.Domain.Entities;
using FirstTry.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FirstTry.Infrastructure.Services;

public class InstitutionService : IInstitutionService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public InstitutionService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<InstitutionResponseDto> CreateInstitutionAsync(CreateInstitutionRequestDto request)
    {
        // Check if UniversalId or Email already exists
        if (await _context.Institutions.AnyAsync(i => i.UniversalId == request.UniversalId))
        {
            return new InstitutionResponseDto { Success = false, Message = "An institution with this Universal ID already exists" };
        }

        if (await _context.Institutions.AnyAsync(i => i.Email == request.Email))
        {
            return new InstitutionResponseDto { Success = false, Message = "An institution with this email already exists" };
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Create institution
            var institution = new Institution
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                UniversalId = request.UniversalId,
                Email = request.Email,
                Description = request.Description,
                Address = request.Address,
                Phone = request.Phone,
                Website = request.Website,
                IsActive = true
            };

            _context.Institutions.Add(institution);
            await _context.SaveChangesAsync();

            // Create users for the institution
            var createdUsers = new List<InstitutionUserDto>();
            foreach (var userRequest in request.Users)
            {
                // Check if user email exists
                var existingUser = await _userManager.FindByEmailAsync(userRequest.Email);
                if (existingUser != null)
                {
                    await transaction.RollbackAsync();
                    return new InstitutionResponseDto { Success = false, Message = $"User with email {userRequest.Email} already exists" };
                }

                var user = new ApplicationUser
                {
                    UserName = userRequest.Email,
                    Email = userRequest.Email,
                    FirstName = userRequest.FirstName,
                    LastName = userRequest.LastName,
                    InstitutionId = institution.Id,
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, userRequest.Password);
                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new InstitutionResponseDto { Success = false, Message = $"Failed to create user {userRequest.Email}: {errors}" };
                }

                await _userManager.AddToRoleAsync(user, "User");

                createdUsers.Add(new InstitutionUserDto
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    IsActive = user.IsActive
                });
            }

            await transaction.CommitAsync();

            return new InstitutionResponseDto
            {
                Success = true,
                Message = "Institution created successfully",
                Institution = new InstitutionDto
                {
                    Id = institution.Id,
                    Name = institution.Name,
                    UniversalId = institution.UniversalId,
                    Email = institution.Email,
                    Description = institution.Description,
                    Address = institution.Address,
                    Phone = institution.Phone,
                    Website = institution.Website,
                    IsActive = institution.IsActive,
                    CreatedAt = institution.CreatedAt,
                    UserCount = createdUsers.Count,
                    Users = createdUsers
                }
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return new InstitutionResponseDto { Success = false, Message = $"An error occurred: {ex.Message}" };
        }
    }

    public async Task<InstitutionResponseDto> GetInstitutionByIdAsync(Guid id)
    {
        var institution = await _context.Institutions
            .Include(i => i.Users)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (institution == null)
        {
            return new InstitutionResponseDto { Success = false, Message = "Institution not found" };
        }

        return new InstitutionResponseDto
        {
            Success = true,
            Institution = MapToDto(institution)
        };
    }

    public async Task<InstitutionResponseDto> GetMyInstitutionAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.Institution)
                .ThenInclude(i => i!.Users)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return new InstitutionResponseDto { Success = false, Message = "User not found" };
        }

        if (user.InstitutionId == null || user.Institution == null)
        {
            return new InstitutionResponseDto { Success = false, Message = "User is not associated with any institution" };
        }

        return new InstitutionResponseDto
        {
            Success = true,
            Institution = MapToDto(user.Institution)
        };
    }

    public async Task<InstitutionListResponseDto> GetAllInstitutionsAsync()
    {
        var institutions = await _context.Institutions
            .Include(i => i.Users)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return new InstitutionListResponseDto
        {
            Success = true,
            Institutions = institutions.Select(MapToDto).ToList(),
            TotalCount = institutions.Count
        };
    }

    public async Task<InstitutionResponseDto> UpdateInstitutionAsync(Guid id, UpdateInstitutionRequestDto request)
    {
        var institution = await _context.Institutions.FindAsync(id);
        if (institution == null)
        {
            return new InstitutionResponseDto { Success = false, Message = "Institution not found" };
        }

        // Check for duplicate UniversalId
        if (await _context.Institutions.AnyAsync(i => i.UniversalId == request.UniversalId && i.Id != id))
        {
            return new InstitutionResponseDto { Success = false, Message = "Another institution with this Universal ID already exists" };
        }

        // Check for duplicate Email
        if (await _context.Institutions.AnyAsync(i => i.Email == request.Email && i.Id != id))
        {
            return new InstitutionResponseDto { Success = false, Message = "Another institution with this email already exists" };
        }

        institution.Name = request.Name;
        institution.UniversalId = request.UniversalId;
        institution.Email = request.Email;
        institution.Description = request.Description;
        institution.Address = request.Address;
        institution.Phone = request.Phone;
        institution.Website = request.Website;
        institution.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return new InstitutionResponseDto
        {
            Success = true,
            Message = "Institution updated successfully",
            Institution = MapToDto(institution)
        };
    }

    public async Task<InstitutionResponseDto> DeleteInstitutionAsync(Guid id)
    {
        var institution = await _context.Institutions.Include(i => i.Users).FirstOrDefaultAsync(i => i.Id == id);
        if (institution == null)
        {
            return new InstitutionResponseDto { Success = false, Message = "Institution not found" };
        }

        // Remove institution reference from users (don't delete users)
        foreach (var user in institution.Users)
        {
            user.InstitutionId = null;
        }

        _context.Institutions.Remove(institution);
        await _context.SaveChangesAsync();

        return new InstitutionResponseDto { Success = true, Message = "Institution deleted successfully" };
    }

    public async Task<InstitutionResponseDto> AddUserToInstitutionAsync(Guid institutionId, AddUserToInstitutionRequestDto request)
    {
        var institution = await _context.Institutions.Include(i => i.Users).FirstOrDefaultAsync(i => i.Id == institutionId);
        if (institution == null)
        {
            return new InstitutionResponseDto { Success = false, Message = "Institution not found" };
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new InstitutionResponseDto { Success = false, Message = $"User with email {request.Email} already exists" };
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            InstitutionId = institutionId,
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return new InstitutionResponseDto { Success = false, Message = $"Failed to create user: {errors}" };
        }

        await _userManager.AddToRoleAsync(user, "User");

        return new InstitutionResponseDto
        {
            Success = true,
            Message = "User added to institution successfully",
            Institution = MapToDto(institution)
        };
    }

    public async Task<InstitutionResponseDto> RemoveUserFromInstitutionAsync(Guid institutionId, string userId)
    {
        var institution = await _context.Institutions.Include(i => i.Users).FirstOrDefaultAsync(i => i.Id == institutionId);
        if (institution == null)
        {
            return new InstitutionResponseDto { Success = false, Message = "Institution not found" };
        }

        var user = institution.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            return new InstitutionResponseDto { Success = false, Message = "User not found in this institution" };
        }

        user.InstitutionId = null;
        await _context.SaveChangesAsync();

        return new InstitutionResponseDto
        {
            Success = true,
            Message = "User removed from institution successfully",
            Institution = MapToDto(institution)
        };
    }

    private InstitutionDto MapToDto(Institution institution)
    {
        return new InstitutionDto
        {
            Id = institution.Id,
            Name = institution.Name,
            UniversalId = institution.UniversalId,
            Email = institution.Email,
            Description = institution.Description,
            Address = institution.Address,
            Phone = institution.Phone,
            Website = institution.Website,
            IsActive = institution.IsActive,
            CreatedAt = institution.CreatedAt,
            UserCount = institution.Users.Count,
            Users = institution.Users.Select(u => new InstitutionUserDto
            {
                UserId = u.Id,
                Email = u.Email!,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                IsActive = u.IsActive,
                AuthenticationType = u.AuthenticationType
            }).ToList()
        };
    }

    public async Task<InstitutionListResponseDto> SearchInstitutionsAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return await GetAllInstitutionsAsync();
        }

        var lowerKeyword = keyword.ToLower();
        var institutions = await _context.Institutions
            .Include(i => i.Users)
            .Where(i => i.Name.ToLower().Contains(lowerKeyword) ||
                       i.UniversalId.ToLower().Contains(lowerKeyword) ||
                       i.Email.ToLower().Contains(lowerKeyword) ||
                       (i.Description != null && i.Description.ToLower().Contains(lowerKeyword)))
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return new InstitutionListResponseDto
        {
            Success = true,
            Institutions = institutions.Select(MapToDto).ToList(),
            TotalCount = institutions.Count
        };
    }

    public async Task<UserResponseDto> UpdateInstitutionUserAsync(Guid institutionId, string userId, UpdateInstitutionUserRequestDto request)
    {
        var institution = await _context.Institutions
            .Include(i => i.Users)
            .FirstOrDefaultAsync(i => i.Id == institutionId);

        if (institution == null)
        {
            return new UserResponseDto { Success = false, Message = "Institution not found" };
        }

        var user = institution.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null)
        {
            return new UserResponseDto { Success = false, Message = "User not found in this institution" };
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.IsActive = request.IsActive;
        user.AuthenticationType = request.AuthenticationType;

        await _context.SaveChangesAsync();

        return new UserResponseDto
        {
            Success = true,
            Message = "User updated successfully",
            User = new InstitutionUserDto
            {
                UserId = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                IsActive = user.IsActive,
                AuthenticationType = user.AuthenticationType
            }
        };
    }
}

