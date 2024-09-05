using Microsoft.AspNetCore.Authorization;
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
    [Route("")]
    public async Task<ActionResult<GetFoldersResponse>> GetFolders()
    {
        GetFoldersQuery query = new() { loggedIn = User.Identity.IsAuthenticated };
        if (User.Identity.IsAuthenticated)
        {
            if (int.TryParse(User.Claims.First(c => c.Type == CustomClaim.UserId).Value, out int userId))
                query.userId = userId;
        }
        var result = await _mediator.Send(query);
        return result.ToJsonResponse();
    }
}