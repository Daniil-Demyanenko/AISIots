using AISIots.DAL;
using Microsoft.AspNetCore.Authentication;

namespace AISIots.Middleware;

public class UserExistenceMiddleware
{
    private readonly RequestDelegate _next;

    public UserExistenceMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userName = context.User.Identity.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                using var scope = context.RequestServices.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IDbRepository>();
                
                var user = await db.GetUserByLoginAsync(userName);
                
                if (user == null)
                {
                    await context.SignOutAsync();
                    context.Response.Redirect("/Main/Login?message=session_expired");
                    return;
                }
            }
        }

        await _next(context);
    }
}

public static class UserExistenceMiddlewareExtensions
{
    public static IApplicationBuilder UseUserExistenceCheck(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserExistenceMiddleware>();
    }
}
