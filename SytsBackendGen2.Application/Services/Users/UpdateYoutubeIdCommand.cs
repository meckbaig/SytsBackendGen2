using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SytsBackendGen2.Application.Common.BaseRequests;
using SytsBackendGen2.Application.Common.Exceptions;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Domain.Entities.Authentification;

namespace SytsBackendGen2.Application.Services.Users;

public record UpdateYoutubeIdCommand : BaseRequest<UpdateYoutubeIdResponse>
{
    public string youtubeId { get; set; }
    public ClaimsPrincipal principal { get; set; }
}

public class UpdateYoutubeIdResponse : BaseResponse
{

}

public class UpdateYoutubeIdCommandValidator : AbstractValidator<UpdateYoutubeIdCommand>
{
    public UpdateYoutubeIdCommandValidator()
    {
        RuleFor(x => x.youtubeId).Length(24);
    }
}

public class UpdateYoutubeIdCommandHandler : IRequestHandler<UpdateYoutubeIdCommand, UpdateYoutubeIdResponse>
{
    private readonly IAppDbContext _context;
    private readonly IJwtProvider _jwtProvider;

    public UpdateYoutubeIdCommandHandler(IAppDbContext context, IJwtProvider jwtProvider)
    {
        _context = context;
        _jwtProvider = jwtProvider;
    }

    public async Task<UpdateYoutubeIdResponse> Handle(UpdateYoutubeIdCommand request, CancellationToken cancellationToken)
    {
        int userId = _jwtProvider.GetUserIdFromClaimsPrincipal(request.principal);

        User? user = await _context.Users
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new Common.Exceptions.ValidationException(
                "JWT token",
                [new ErrorItem($"Unable to find user with id {userId}.", ValidationErrorCode.EntityIdValidator)]);
        }

        user.YoutubeId = request.youtubeId;
        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateYoutubeIdResponse();
    }
}
