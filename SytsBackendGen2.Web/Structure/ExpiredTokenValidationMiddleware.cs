using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace SytsBackendGen2.Web.Structure
{
    /// <summary>
    /// Middleware for validating expired tokens in incoming requests.
    /// </summary>
    /// <remarks>Ignores endpoints with the Authorize attribute.</remarks>
    public class ExpiredTokenValidationMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpiredTokenValidationMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        public ExpiredTokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Asynchronously validates expired tokens in incoming requests.
        /// </summary>
        /// <param name="context">The HTTP context of the incoming request.</param>
        /// <returns>A task representing the completion of handling the request.</returns>
        /// <remarks>Ignores endpoints with the Authorize attribute.</remarks>
        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the endpoint has the Authorize attribute, if so skip token validation
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var skipValidation = endpoint.Metadata.GetMetadata<AuthorizeAttribute>();
                if (skipValidation != null)
                {
                    await _next(context);
                    return;
                }
            }

            // Get the token from the Authorization header
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            // If a token is present, validate it
            if (token != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                // Get the expiration time from the token
                var expString = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp)?.Value;
                if (!long.TryParse(expString, out long expiresInSeconds))
                {
                    // If the expiration time cannot be parsed, return a 401 Unauthorized status code
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                DateTime expires = DateTimeOffset.FromUnixTimeSeconds(expiresInSeconds).UtcDateTime;
                if (expires < DateTime.UtcNow)
                {
                    // If the token has expired, return a 401 Unauthorized status code
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
            }

            // If the token is valid or not present, continue with the next middleware in the pipeline
            await _next(context);
        }
    }
}
