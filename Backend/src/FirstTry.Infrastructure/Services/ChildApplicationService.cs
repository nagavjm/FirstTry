using FirstTry.Application.Common.Interfaces;
using FirstTry.Application.Features.ChildApplication.DTOs;
using FirstTry.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FirstTry.Infrastructure.Services;

public class ChildApplicationService : IChildApplicationService
{
    private readonly ApplicationDbContext _context;

    public ChildApplicationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ChildApplicationListResponseDto> GetAllApplicationsAsync(string userId)
    {
        var applications = await _context.ChildApplications
            .Where(a => a.IsActive)
            .OrderBy(a => a.DisplayOrder)
            .ToListAsync();

        var userAccess = await _context.UserApplicationAccesses
            .Where(ua => ua.UserId == userId && ua.HasAccess)
            .Select(ua => ua.ChildApplicationId)
            .ToListAsync();

        var appDtos = applications.Select(app => new ChildApplicationDto
        {
            Id = app.Id,
            Name = app.Name,
            Description = app.Description,
            IconUrl = app.IconUrl,
            LaunchUrl = app.LaunchUrl,
            DisplayOrder = app.DisplayOrder,
            HasAccess = userAccess.Contains(app.Id)
        }).ToList();

        return new ChildApplicationListResponseDto
        {
            Success = true,
            Applications = appDtos
        };
    }

    public async Task<ChildApplicationListResponseDto> GetAccessibleApplicationsAsync(string userId)
    {
        var accessibleApps = await _context.UserApplicationAccesses
            .Include(ua => ua.ChildApplication)
            .Where(ua => ua.UserId == userId && ua.HasAccess && ua.ChildApplication.IsActive)
            .OrderBy(ua => ua.ChildApplication.DisplayOrder)
            .Select(ua => new ChildApplicationDto
            {
                Id = ua.ChildApplication.Id,
                Name = ua.ChildApplication.Name,
                Description = ua.ChildApplication.Description,
                IconUrl = ua.ChildApplication.IconUrl,
                LaunchUrl = ua.ChildApplication.LaunchUrl,
                DisplayOrder = ua.ChildApplication.DisplayOrder,
                HasAccess = true
            })
            .ToListAsync();

        return new ChildApplicationListResponseDto
        {
            Success = true,
            Applications = accessibleApps
        };
    }
}

