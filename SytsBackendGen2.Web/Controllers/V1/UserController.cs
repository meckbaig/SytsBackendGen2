using Microsoft.AspNetCore.Authorization;
using SytsBackendGen2.Application.Services.Users;
using SytsBackendGen2.Domain.Enums;
using SytsBackendGen2.Infrastructure.Authentification.Jwt;
using SytsBackendGen2.Infrastructure.Authentification.Permissions;

namespace SytsBackendGen2.Web.Controllers.V1;

[Route("api/v{version:ApiVersion}/[controller]")]
[ApiController]
[ApiVersion("1")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [HasPermission(Permission.PrivateDataEditor)]
    [Route("UpdateYoutubeId")]
    public async Task<ActionResult<UpdateYoutubeIdResponse>> UpdateYoutubeId([FromBody] UpdateYoutubeIdCommand command)
    {
        if (int.TryParse(User.Claims.First(c => c.Type == CustomClaim.UserId).Value, out int userId))
            command.SetUserId(userId);
        var result = await _mediator.Send(command);
        return result.ToJsonResponse();
    }

    [HttpPost]
    [HasPermission(Permission.PrivateDataEditor)]
    [Route("UpdateSubChannels")]
    public async Task<ActionResult<UpdateSubChannelsResponse>> UpdateSubChannels([FromBody] UpdateSubChannelsCommand command)
    {
        if (int.TryParse(User.Claims.First(c => c.Type == CustomClaim.UserId).Value, out int userId))
            command.SetUserId(userId);
        var result = await _mediator.Send(command);
        return result.ToJsonResponse();
    }
}