using Hangfire;
using MyAi.Api.Extensions;
using MyAi.Infrastructure;
using MyAi.Infrastructure.BackgroundJobs;
using MyAi.Infrastructure.Logging;
using MyAi.Infrastructure.Persistence;
using MyAi.Infrastructure.Persistence.Seeding;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting MyAi API");

    var builder = WebApplication.CreateBuilder(args);

    // ── Logging ────────────────────────────────────────────────────────────────
    builder.Host.UseSerilog(SerilogConfiguration.Configure);

    // ── Layers ─────────────────────────────────────────────────────────────────
    builder.Services
        .AddApplicationServices()
        .AddInfrastructureServices(builder.Configuration)
        .AddApiServices(builder.Configuration);

    var app = builder.Build();

    // ── Database migration + seed (startup) ────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();

        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync();

            if (canConnect)
            {
                await dbContext.Database.MigrateAsync();
                await seeder.SeedAsync(
                    builder.Configuration["SeedAdmin:Email"],
                    builder.Configuration["SeedAdmin:Password"],
                    builder.Configuration["SeedAdmin:DisplayName"]);
            }
            else
            {
                Log.Fatal("Database unreachable — skipping migration/seed. Health checks will report unhealthy.");
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Database migration/seed failed — API will start in a degraded state.");
        }
    }

    // ── Pipeline ───────────────────────────────────────────────────────────────
    app.UseApiMiddleware();
    app.UseSwaggerInDevelopment(app.Environment);

    app.MapControllers();
    app.MapHangfireDashboard("/hangfire");

    // ── Recurring jobs ─────────────────────────────────────────────────────────
    RecurringJob.AddOrUpdate<AudioCleanupJob>(
        "audio-cleanup", job => job.ExecuteAsync(CancellationToken.None), Cron.Daily);
    RecurringJob.AddOrUpdate<ExpiredSubscriptionJob>(
        "expired-subscriptions", job => job.ExecuteAsync(CancellationToken.None), Cron.Daily(4));
    RecurringJob.AddOrUpdate<RefreshTokenCleanupJob>(
        "refresh-token-cleanup", job => job.ExecuteAsync(CancellationToken.None), Cron.Weekly);

    Log.Information("MyAi API started on {Urls}", string.Join(", ", app.Urls));

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "MyAi API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Exposes the implicit Program class for WebApplicationFactory integration tests.
public partial class Program { }
