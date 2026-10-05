using EMIT.Infrastructure;
using EMIT.Infrastructure.Data;
using EMIT.Infrastructure.Middleware;
using EMIT.Infrastructure.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Render / proxies gratuits : respecte X-Forwarded-Proto/For pour HTTPS + IP client
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UserActivityMiddleware>();

app.MapStaticAssets();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Timetable}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var seedService = scope.ServiceProvider.GetRequiredService<SeedService>();
        await seedService.SeedAsync();
        logger.LogInformation("Database migration + seed OK");
    }
    catch (Exception ex)
    {
        // Ne crash pas le container : Render affichera l'erreur dans les logs
        // au lieu d'un segfault 139. L'app demarre, la DB pourra etre fixee via env vars.
        logger.LogError(ex, "Seed failed (check ConnectionStrings__DefaultConnection, sslmode=require). App starts without seed.");
    }
}

app.Run();
