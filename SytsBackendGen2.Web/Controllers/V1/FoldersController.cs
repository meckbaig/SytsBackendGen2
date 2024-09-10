using SytsBackendGen2.Application.Services.Folders;
using SytsBackendGen2.Domain.Enums;
using SytsBackendGen2.Infrastructure.Authentification.Jwt;
using SytsBackendGen2.Infrastructure.Authentification.Permissions;

namespace SytsBackendGen2.Web.Controllers.V1;

[Route("api/v{version:ApiVersion}/[controller]")]
[ApiController]
[ApiVersion("1")]
public class FoldersController : ControllerBase
{
    private readonly IMediator _mediator;

    public FoldersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<GetFoldersResponse>> GetFolders()
    {
        GetFoldersQuery query = new();
        if (User.Identity.IsAuthenticated)
        {
            if (int.TryParse(User.Claims.First(c => c.Type == CustomClaim.UserId).Value, out int userId))
                query.SetUserId(userId);
        }
        var result = await _mediator.Send(query);
        return result.ToJsonResponse();
    }

    [HttpGet]
    [Route("{guid}")]
    public async Task<ActionResult<GetFolderResponse>> GetFolder(Guid guid, [FromQuery] GetFolderQuery query)
    {
        query.SetFolderGuid(guid);
        if (User.Identity.IsAuthenticated)
        {
            if (int.TryParse(User.Claims.First(c => c.Type == CustomClaim.UserId).Value, out int userId))
                query.SetUserId(userId);
        }
        var result = await _mediator.Send(query);
        return result.ToJsonResponse();
    }

    [HttpPut]
    [HasPermission(Permission.PrivateDataEditor)]
    [Route("{guid}")]
    public async Task<ActionResult<UpdateFolderResponse>> UpdateFolder(Guid guid, [FromBody] UpdateFolderCommand command)
    {
        command.SetFolderGuid(guid);
        if (int.TryParse(User.Claims.First(c => c.Type == CustomClaim.UserId).Value, out int userId))
            command.SetUserId(userId);
        var result = await _mediator.Send(command);
        return result.ToJsonResponse();
    }

    [HttpPost]
    [HasPermission(Permission.PrivateDataEditor)]
    public async Task<ActionResult<CreateFolderResponse>> CreateFolder(CreateFolderCommand command)
    {
        if (int.TryParse(User.Claims.First(c => c.Type == CustomClaim.UserId).Value, out int userId))
            command.SetUserId(userId);
        var result = await _mediator.Send(command);
        return result.ToJsonResponse();
    }

    [HttpDelete]
    [HasPermission(Permission.PrivateDataEditor)]
    [Route("{guid}")]
    public async Task<ActionResult<DeleteFolderResponse>> DeleteFolder(Guid guid)
    {
        var command = new DeleteFolderCommand() { guid = guid };
        if (int.TryParse(User.Claims.First(c => c.Type == CustomClaim.UserId).Value, out int userId))
            command.SetUserId(userId);
        var result = await _mediator.Send(command);
        return result.ToJsonResponse();
    }

}