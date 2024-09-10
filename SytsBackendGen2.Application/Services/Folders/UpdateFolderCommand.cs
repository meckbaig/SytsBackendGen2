using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.BaseRequests.AuthentificatedRequest;
using SytsBackendGen2.Application.Common.Exceptions;
using SytsBackendGen2.Application.Common.Extensions.Caching;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Application.Extensions.Validation;
using SytsBackendGen2.Domain.Entities;

namespace SytsBackendGen2.Application.Services.Folders;

public record UpdateFolderCommand : BaseAuthentificatedRequest<UpdateFolderResponse>
{
    public required FolderEditDto folder { get; set; }
    internal override int userId { get; set; }
    internal Guid guid { get; set; }

    public void SetFolderGuid(Guid guid) => this.guid = guid;
}

public class UpdateFolderResponse : BaseResponse
{
	public required FolderDto Folder { get; set; }
}

public class UpdateFolderCommandValidator : AbstractValidator<UpdateFolderCommand>
{
    public UpdateFolderCommandValidator(IAppDbContext context)
    {
        RuleFor(x => x.userId).MustHaveValidUserId(context);
        RuleFor(x => x.guid).MustHaveValidFolderGuid(context);
        RuleFor(x => x.folder).SetValidator(GetValidatorForFolder(context));
    }

    private IValidator<FolderEditDto> GetValidatorForFolder(IAppDbContext context)
    {
        return (IValidator<FolderEditDto>)Activator.CreateInstance(FolderEditDto.GetValidatorType(), [context]);
    }
}

public class UpdateFolderCommandHandler : IRequestHandler<UpdateFolderCommand, UpdateFolderResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IDistributedCache _cache;

    public UpdateFolderCommandHandler(IAppDbContext context, IMapper mapper, IDistributedCache cache)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<UpdateFolderResponse> Handle(UpdateFolderCommand request, CancellationToken cancellationToken)
    {
        Folder folder = (await _context.Folders.FirstOrDefaultAsync(f => f.Guid == request.guid, cancellationToken))!;
        if (folder.UserId != request.userId)
        {
            throw new ForbiddenAccessException(
                nameof(request.guid),
                [new ErrorItem($"User '{request.userId}' is not owner of folder '{request.guid}'.",
                    ForbiddenAccessErrorCode.ForbiddenAccessValidator)]);
        }
        var previousSubChannels = folder.SubChannelsJson;

        _mapper.Map(request.folder, folder);
        if (previousSubChannels != folder.SubChannelsJson)
        {
            folder.LastChannelsUpdate = DateTime.UtcNow;
            folder.LastVideoId = null;
            _cache.RemoveFromCache(folder.Guid.ToString());
        }
        _context.Folders.Update(folder);
        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateFolderResponse() { Folder = _mapper.Map<FolderDto>(folder) };
    }
}
