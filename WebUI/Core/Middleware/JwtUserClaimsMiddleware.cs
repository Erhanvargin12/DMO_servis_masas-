using WebUI.Core.Security;

namespace WebUI.Core.Middleware;

public class JwtUserClaimsMiddleware
{
    private readonly RequestDelegate _next;

    public JwtUserClaimsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var token = JwtClaimsHelper.GetAccessToken(context);
            context.User = JwtClaimsHelper.HydratePrincipal(context.User, token);
        }

        await _next(context);
    }
}

public static class JwtUserClaimsMiddlewareExtensions
{
    public static IApplicationBuilder UseJwtUserClaims(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JwtUserClaimsMiddleware>();
    }
}
