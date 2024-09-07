using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.BaseRequests.AuthentificatedRequest;
using SytsBackendGen2.Application.Common.Exceptions;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Application.Extensions.Validation;
using SytsBackendGen2.Domain.Enums;

namespace SytsBackendGen2.Application.Services.Folders;

public record GetFolderQuery : BaseAuthentificatedRequest<GetFolderResponse>
{
    public Guid guid { get; set; }
    internal override bool loggedIn { get; set; }
    internal override int userId { get; set; }

}

public class GetFolderResponse : BaseResponse
{
    public required FolderDto Folder { get; set; }
    public required List<VideoDto> Videos { get; set; }
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

    public GetFolderQueryHandler(IAppDbContext context, IMapper mapper, IVideoFetcher videoFetcher)
    {
        _context = context;
        _mapper = mapper;
        _videoFetcher = videoFetcher;
    }

    public async Task<GetFolderResponse> Handle(GetFolderQuery request, CancellationToken cancellationToken)
    {
        var folder = await _context.Folders
            .Include(f => f.Access)
            .FirstOrDefaultAsync(f => f.Guid == request.guid);

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

        var folderDto = _mapper.Map<FolderDto>(folder);


        await _videoFetcher.Fetch(folderDto.SubChannels, folderDto.YoutubeFolders, folderDto.ChannelsCount);
        var dynamicVideosList = _videoFetcher.ToList(out string firstVideoId, folder.LastVideoId);
        if (firstVideoId != null)
        {
            folder.LastVideoId = firstVideoId;
            folder.LastVideosAccess = folderDto.LastVideosAccess = DateTime.UtcNow;
            _context.SaveChanges();
        }

        var response = new GetFolderResponse
        {
            Folder = folderDto,
            Videos = _mapper.Map<List<VideoDto>>(dynamicVideosList)
        };

        return response;
    }
}
