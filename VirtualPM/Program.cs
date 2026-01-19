using Hangfire;
using Hangfire.MemoryStorage;
using VirtualPM.Jobs;
using VirtualPM.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddHttpClient(); // Required for AsanaService
builder.Services.AddScoped<SlackService>();
builder.Services.AddScoped<ClaudeService>();
builder.Services.AddScoped<AsanaService>();

string cronSchedule = builder.Configuration.GetValue<string>("VirtualPM:CronSchedule") ?? "0 13 * * 0-4";

// Configure Hangfire
builder.Services.AddHangfire(c =>
{
    RecurringJobOptions options = new() { TimeZone = TimeZoneInfo.Local };
    c.UseMemoryStorage();
    RecurringJob.AddOrUpdate<CheckDailyUpdatesJob>(
        nameof(CheckDailyUpdatesJob),
        j => j.ExecuteAsync(),
        cronSchedule,
        options);
});
builder.Services.AddHangfireServer();

WebApplication app = builder.Build();

app.MapGet("/", () => "🤖 Virtual PM - Ready to track daily updates!");
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [] // No auth
});
app.Run();