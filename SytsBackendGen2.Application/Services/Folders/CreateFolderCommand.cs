using AutoMapper;
using FluentValidation;
using MediatR;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.BaseRequests.AuthentificatedRequest;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.Extensions.Validation;
using SytsBackendGen2.Domain.Entities;

namespace SytsBackendGen2.Application.Services.Folders;

public record CreateFolderCommand : BaseAuthentificatedRequest<CreateFolderResponse>
{
    public required string name { get; set; }
    internal override int userId { get; set; }
}

public class CreateFolderResponse : BaseResponse
{
	
}

public class CreateFolderCommandValidator : AbstractValidator<CreateFolderCommand>
{
    public CreateFolderCommandValidator(IAppDbContext context)
    {
        RuleFor(x => x.name).Length(1, 50);
        RuleFor(x => x.userId).MustHaveValidUserId(context);
    }
}

public class CreateFolderCommandHandler : IRequestHandler<CreateFolderCommand, CreateFolderResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public CreateFolderCommandHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CreateFolderResponse> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
    {
        Folder folder = new(request.userId, request.name);
        _context.Folders.Add(folder);
        await _context.SaveChangesAsync();

        return new CreateFolderResponse();
    }
}
