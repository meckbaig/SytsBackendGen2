using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.BaseRequests.AuthentificatedRequest;
using SytsBackendGen2.Application.Common.Exceptions;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.Extensions.Validation;
using SytsBackendGen2.Domain.Entities;

namespace SytsBackendGen2.Application.Services.Folders;

public record DeleteFolderCommand : BaseAuthentificatedRequest<DeleteFolderResponse>
{
    public Guid guid { get; set; }
    internal override int userId { get; set; }
}

public class DeleteFolderResponse : BaseResponse
{
	
}

public class DeleteFolderCommandValidator : AbstractValidator<DeleteFolderCommand>
{
    public DeleteFolderCommandValidator(IAppDbContext context)
    {
        RuleFor(x => x.userId).MustHaveValidUserId(context);
        RuleFor(x => x.guid).MustHaveValidFolderGuid(context);
    }
}

public class DeleteFolderCommandHandler : IRequestHandler<DeleteFolderCommand, DeleteFolderResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public DeleteFolderCommandHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DeleteFolderResponse> Handle(DeleteFolderCommand request, CancellationToken cancellationToken)
    {
        Folder folder = (await _context.Folders.FirstOrDefaultAsync(f => f.Guid == request.guid, cancellationToken))!;
        if (folder.UserId != request.userId) 
         {
            throw new ForbiddenAccessException(
                nameof(request.guid),
                [new ErrorItem($"User '{request.userId}' is not owner of folder '{request.guid}'.", 
                    ForbiddenAccessErrorCode.ForbiddenAccessValidator)]);
        }

        _context.Folders.Remove(folder);
        await _context.SaveChangesAsync();

        return new DeleteFolderResponse();
    }
}
