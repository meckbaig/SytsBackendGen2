using Microsoft.AspNetCore.Authorization;
using SytsBackendGen2.Application.Services.Users;
using SytsBackendGen2.Domain.Enums;
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
    public async Task<ActionResult<UpdateYoutubeIdResponse>> UpdateYoutubeId([FromBody] UpdateYoutubeId body)
    {
        var command = new UpdateYoutubeIdCommand { youtubeId = body.youtubeId, principal = User };
        var result = await _mediator.Send(command);
        return result.ToJsonResponse();
    }
}

public sealed record UpdateYoutubeId(string youtubeId);