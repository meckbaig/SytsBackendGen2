using Microsoft.AspNetCore.Authorization;
using SytsBackendGen2.Application.Services.Authorization;

namespace SytsBackendGen2.Web.Controllers.V1;

[Route("api/v{version:ApiVersion}/[controller]")]
[ApiController]
[ApiVersion("1")]
public class AuthorizationController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthorizationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Route("")]
    public async Task<ActionResult<AuthorizeUserResponse>> Authorize([FromQuery] AuthorizeUserCommand query)
    {
        var result = await _mediator.Send(query);
        return result.ToJsonResponse();
    }

    [HttpPost]
    [Authorize]
    [Route("RefreshToken")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(string refreshToken)
    {
        var command = new RefreshTokenCommand { refreshToken = refreshToken, principal = User };
        var result = await _mediator.Send(command);
        return result.ToJsonResponse();
    }
}