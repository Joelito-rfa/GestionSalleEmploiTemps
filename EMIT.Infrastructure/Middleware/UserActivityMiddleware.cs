using EMIT.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EMIT.Infrastructure.Middleware;

public class UserActivityMiddleware
{
    private readonly RequestDelegate _next;

    public UserActivityMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userManager = context.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            var userId = userManager.GetUserId(context.User);
            if (userId != null)
            {
                var session = await dbContext.UserSessions
                    .Where(s => s.UserId == userId && s.IsActive)
                    .OrderByDescending(s => s.LoginAt)
                    .FirstOrDefaultAsync();

                if (session != null)
                {
                    var timeSinceLastActivity = DateTime.UtcNow - session.LastActivityAt;
                    if (timeSinceLastActivity.TotalMinutes >= 1)
                    {
                        session.LastActivityAt = DateTime.UtcNow;
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
