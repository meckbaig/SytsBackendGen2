using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Newtonsoft.Json.Linq;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.BaseRequests.AuthentificatedRequest;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.DTOs.Folders;
using SytsBackendGen2.Application.Extensions.Validation;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace SytsBackendGen2.Application.Services.Users;

public record UpdateSubChannelsV1_1Command : BaseAuthentificatedRequest<UpdateSubChannelsV1_1Response>
{
    internal override int userId { get; set; }
}

public class UpdateSubChannelsV1_1Response : BaseResponse
{
    public required List<SubChannelDto> SubChannels { get; set; }
    public required DateTimeOffset LastChannelsUpdate { get; set; }
}

public class UpdateSubChannelsV1_1CommandValidator : AbstractValidator<UpdateSubChannelsV1_1Command>
{
    public UpdateSubChannelsV1_1CommandValidator(IAppDbContext context)
    {
        RuleFor(x => x.userId).MustHaveValidUserId(context);
    }
}

public class UpdateSubChannelsV1_1CommandHandler : IRequestHandler<UpdateSubChannelsV1_1Command, UpdateSubChannelsV1_1Response>
{
    private readonly IAppDbContext _context;
    private readonly IMediator _mediator;
    private readonly IGoogleAuthProvider _googleAuthProvider;

    public UpdateSubChannelsV1_1CommandHandler(
        IAppDbContext context,
        IMediator mediator,
        IGoogleAuthProvider googleAuthProvider)
    {
        _context = context;
        _mediator = mediator;
        _googleAuthProvider = googleAuthProvider;
    }

    public async Task<UpdateSubChannelsV1_1Response> Handle(
        UpdateSubChannelsV1_1Command request,
        CancellationToken cancellationToken)
    {
        User user = _context.Users.FirstOrDefault(u => u.Id == request.userId)!;
        HashSet<SubChannelDto> subChannels = new();

        subChannels = await GetSubChannels(user.YoutubeId, subChannels, null);

        var command = new UpdateSubChannelsCommand
        {
            userId = request.userId,
            channels = subChannels.ToList()
        };
        var response = await _mediator.Send(command);

        return new UpdateSubChannelsV1_1Response
        {
            SubChannels = subChannels.ToList(), 
            LastChannelsUpdate = response.LastChannelsUpdate
        };
    }

    private async Task<HashSet<SubChannelDto>> GetSubChannels(
        string channelId,
        HashSet<SubChannelDto> subChannels,
        string? nextPageToken = null)
    {
        (List<SubChannelDto> subChannelsResponse, nextPageToken)
            = await _googleAuthProvider.GetSubChannels(channelId, nextPageToken);

        subChannels = [..subChannels, ..subChannelsResponse];

        if (nextPageToken != null)
            return await GetSubChannels(channelId, subChannels, nextPageToken);
        return subChannels;
    }
}
