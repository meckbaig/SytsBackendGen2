﻿using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SytsBackendGen2.Application.Common.Attributes;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.BaseRequests.AuthentificatedRequest;
using SytsBackendGen2.Application.Common.Exceptions;
using SytsBackendGen2.Application.Common.Extensions.Caching;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Application.Extensions.Validation;
using SytsBackendGen2.Domain.Entities;
using SytsBackendGen2.Domain.Enums;

namespace SytsBackendGen2.Application.Services.Folders;

public record GetFolderQuery : BaseAuthentificatedRequest<GetFolderResponse>
{
    internal Guid guid { get; set; }
    public bool info { get; set; } = false;
    public bool forceRefresh { get; set; } = false;
    internal override bool loggedIn { get; set; }
    internal override int userId { get; set; }

    public void SetFolderGuid(Guid guid) => this.guid = guid;
    public override string GetKey() => guid.ToString();
}

public class GetFolderResponse : BaseResponse
{
    [IgnoreIfNull]
    public List<VideoDto>? Videos { get; set; }
    public required FolderDto Folder { get; set; }
}

public class GetFolderQueryValidator : AbstractValidator<GetFolderQuery>
{
    public GetFolderQueryValidator(IAppDbContext context)
    {
        RuleFor(x => x.userId).MustHaveValidUserId(context);
        RuleFor(x => x.guid).MustHaveValidFolderGuid(context);
    }
}

public class GetFolderQueryHandler : IRequestHandler<GetFolderQuery, GetFolderResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IVideoFetcher _videoFetcher;
    private readonly IDistributedCache _cache;

    public GetFolderQueryHandler(
        IAppDbContext context,
        IMapper mapper,
        IVideoFetcher videoFetcher,
        IDistributedCache cache)
    {
        _context = context;
        _mapper = mapper;
        _videoFetcher = videoFetcher;
        _cache = cache;
    }

    public async Task<GetFolderResponse> Handle(GetFolderQuery request, CancellationToken cancellationToken)
    {
        var folder = await _context.Folders
            .Include(f => f.Access)
            .FirstOrDefaultAsync(f => f.Guid == request.guid);
        ValidateFolder(request, folder);

        FolderDto folderDto = _mapper.Map<FolderDto>(folder);
        List<VideoDto> videos = null;

        if (!request.info)
        {
            Task<List<dynamic>> fetchTask = Task.Run(async () =>
            {
                await _videoFetcher.Fetch(folderDto.SubChannels, folderDto.YoutubeFolders, folderDto.ChannelsCount);
                return _videoFetcher.ToList(folder.LastVideoId);
            });

            var fetchResult = await _cache.GetOrCreateAsync(
                request.GetKey(),
                () => fetchTask,
                _mapper.Map<List<VideoDto>>,
                cancellationToken, 
                request.forceRefresh);
            videos = fetchResult.Data;

            if (videos?.FirstOrDefault()?.Id != null && !fetchResult.FetchedFromCache)
            {
                folder.LastVideoId = videos!.FirstOrDefault()!.Id;
                folder.LastVideosAccess = folderDto.LastVideosAccess = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        return new GetFolderResponse
        {
            Videos = videos,
            Folder = folderDto
        };
    }

    private static void ValidateFolder(GetFolderQuery request, Folder? folder)
    {
        if (folder == null)
        {
            throw new Common.Exceptions.ValidationException(
                nameof(request.guid),
                [new ErrorItem($"Folder with guid '{request.guid}' doesn't exist.",
                    ValidationErrorCode.PropertyDoesNotExistValidator)]);
        }
        if (folder.Access == AccessEnum.Private && folder.UserId != request.userId)
        {
            throw new ForbiddenAccessException(
                "JWT token",
                [new ErrorItem("User doesn't have access to this folder.", ForbiddenAccessErrorCode.ForbiddenAccessValidator)]);
        }
    }
}
