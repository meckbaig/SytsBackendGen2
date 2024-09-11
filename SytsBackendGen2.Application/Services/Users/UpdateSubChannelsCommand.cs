using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.BaseRequests.AuthentificatedRequest;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Application.Extensions.Validation;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace SytsBackendGen2.Application.Services.Users;

public record UpdateSubChannelsCommand : BaseAuthentificatedRequest<UpdateSubChannelsResponse>
{
    public required List<SubChannelDto> channels { get; set; }
    internal override int userId { get; set; }
}

public class UpdateSubChannelsResponse : BaseResponse
{
    public DateTimeOffset LastChannelsUpdate { get; set; }
}

public class UpdateSubChannelsCommandValidator : AbstractValidator<UpdateSubChannelsCommand>
{
    public UpdateSubChannelsCommandValidator(IAppDbContext context)
    {
        RuleFor(x => x.userId).MustHaveValidUserId(context);
    }
}

public class UpdateSubChannelsCommandHandler : IRequestHandler<UpdateSubChannelsCommand, UpdateSubChannelsResponse>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public UpdateSubChannelsCommandHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UpdateSubChannelsResponse> Handle(UpdateSubChannelsCommand request, CancellationToken cancellationToken)
    {
        User user = (await _context.Users.FirstOrDefaultAsync(u => u.Id == request.userId, cancellationToken))!;

        user.SetSubChannelsJson(
            JsonConvert.SerializeObject(
                request.channels,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }
            ));
        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateSubChannelsResponse() { LastChannelsUpdate = (DateTimeOffset)user.LastChannelsUpdate!};
    }
}
