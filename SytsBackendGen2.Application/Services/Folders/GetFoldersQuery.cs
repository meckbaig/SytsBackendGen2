using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Domain.Entities;
using SytsBackendGen2.Domain.Enums;

namespace SytsBackendGen2.Application.Services.Folders;

public record GetFoldersQuery : BaseRequest<GetFoldersResponse>
{
    public int userId { get; set; }
    public bool loggedIn { get; set; }
}

public class GetFoldersResponse : BaseResponse
{
    public List<FolderDto> PersonalFolders { get; set; }
    public List<FolderDto> PublicFolders { get; set; }
}

public class GetFoldersQueryValidator : AbstractValidator<GetFoldersQuery>
{
    public GetFoldersQueryValidator(IAppDbContext context)
    {
        RuleFor(x => x).MustHaveZeroIdWhenNotLoggedIn();
        RuleFor(x => x).MustHaveValidIdWhenLoggedIn(context);
    }
}

internal static class GetFoldersQueryValidationExpressions
{
    public static IRuleBuilderOptions<GetFoldersQuery, GetFoldersQuery> MustHaveZeroIdWhenNotLoggedIn
        (this IRuleBuilder<GetFoldersQuery, GetFoldersQuery> ruleBuilder)
    {
        return ruleBuilder.Must((q, p) => HaveZeroIdWhenNotLoggedIn(p.userId, p.loggedIn))
            .WithMessage((q, p) => $"Internal error: User is no logged in, but user Id is present")
            .WithErrorCode("NotLoggedUserWuthId");
    }

    private static bool HaveZeroIdWhenNotLoggedIn(int userId, bool loggedIn)
    {
        if (!loggedIn)
            return userId == 0;
        return true;
    }

    public static IRuleBuilderOptions<GetFoldersQuery, GetFoldersQuery> MustHaveValidIdWhenLoggedIn
        (this IRuleBuilder<GetFoldersQuery, GetFoldersQuery> ruleBuilder, IAppDbContext context)
    {
        return ruleBuilder.Must((q, p) => HaveValidIdWhenLoggedIn(p.userId, p.loggedIn, context))
            .WithMessage((q, p) => $"User Id is not valid")
            .WithErrorCode("NotValidUserId");
    }

    private static bool HaveValidIdWhenLoggedIn(int userId, bool loggedIn, IAppDbContext context)
    {
        if (loggedIn)
        {
            if (userId > 0)
                return context.Users.Any(u => u.Id == userId);
            return false;
        }
        return true;
    }
}

public class GetFoldersQueryHandler : IRequestHandler<GetFoldersQuery, GetFoldersResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public GetFoldersQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GetFoldersResponse> Handle(GetFoldersQuery request, CancellationToken cancellationToken)
    {
        List<Folder> personalFolders = new();
        if (request.loggedIn)
        {
            personalFolders = await GetPrivateFolders(request.userId);
        }
        var publicFolders = await GetPublicFolders(request.userId);

        return new()
        {
            PersonalFolders = personalFolders.Select(_mapper.Map<FolderDto>).ToList(),
            PublicFolders = publicFolders.Select(_mapper.Map<FolderDto>).ToList()
        };
    }

    private async Task<List<Folder>> GetPrivateFolders(int userId)
    {
        return await _context.Folders
            .Include(f => f.Access)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.LastChannelsUpdate)
            .ToListAsync();
    }

    private async Task<List<Folder>> GetPublicFolders(int userId)
    {
        return await _context.Folders
            .Include(f => f.Access)
            .Where(f => (AccessEnum)f.AccessId == AccessEnum.Public && f.UserId != userId)
            .Take(50)
            .ToListAsync();
    }
}
