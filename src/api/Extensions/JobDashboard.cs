using System.Security.Claims;
using Hangfire;
using Hangfire.Dashboard;

namespace albumica.Extensions;

public static class JobDashboard
{
    public static void UseJobDashboard(this IApplicationBuilder app)
    {
        app.UseHangfireDashboard("/jobs", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthFilter() },
            DashboardTitle = "albumica jobs",
            DisplayStorageConnectionString = false,
        });
    }
}

public class HangfireAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var ctx = context.GetHttpContext();
        if (!ctx.User.Identity?.IsAuthenticated ?? true)
            return false;

        var role = ctx.User.FindFirst(ClaimTypes.Role)?.Value;
        return role == C.ADMIN_ROLE;
    }
}