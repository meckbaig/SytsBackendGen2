using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.BaseRequests.AuthentificatedRequest;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Application.Extensions.Validation;
using SytsBackendGen2.Domain.Entities;
using SytsBackendGen2.Domain.Enums;

namespace SytsBackendGen2.Application.Services.Folders;

public record GetFoldersQuery : BaseAuthentificatedRequest<GetFoldersResponse>
{
    internal override int userId { get; set; }
    internal override bool loggedIn { get; set; }
}

public class GetFoldersResponse : BaseResponse
{
    public required List<FolderDto> PersonalFolders { get; set; }
    public required List<FolderDto> PublicFolders { get; set; }
}

public class GetFoldersQueryValidator : AbstractValidator<GetFoldersQuery>
{
    public GetFoldersQueryValidator(IAppDbContext context)
    {
        RuleFor(x => x.userId).MustHaveValidUserId(context);
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
            personalFolders = await GetPrivateFolders(request.userId, cancellationToken);
        }
        var publicFolders = await GetPublicFolders(request.userId, cancellationToken);

        return new()
        {
            PersonalFolders = personalFolders.Select(_mapper.Map<FolderDto>).ToList(),
            PublicFolders = publicFolders.Select(_mapper.Map<FolderDto>).ToList()
        };
    }

    private async Task<List<Folder>> GetPrivateFolders(int userId, CancellationToken cancellationToken)
    {
        return await _context.Folders
            .Include(f => f.Access)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.LastChannelsUpdate)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<Folder>> GetPublicFolders(int userId, CancellationToken cancellationToken)
    {
        return await _context.Folders
            .Include(f => f.Access)
            .Where(f => (AccessEnum)f.AccessId == AccessEnum.Public && f.UserId != userId)
            .Take(50)
            .ToListAsync(cancellationToken);
    }
}
