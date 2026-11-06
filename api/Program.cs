using System.Net;

var builder = WebApplication.CreateBuilder(args);

var corsOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS");
string[]? allowedOrigins = null;
if (!string.IsNullOrWhiteSpace(corsOrigins))
{
    allowedOrigins = corsOrigins
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(static origin => origin.TrimEnd('/'))
        .Where(static origin => origin.Length > 0)
        .ToArray();

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyHeader().AllowAnyMethod();
            if (allowedOrigins.Contains("*", StringComparer.Ordinal))
            {
                policy.AllowAnyOrigin();
            }
            else
            {
                policy.WithOrigins(allowedOrigins);
            }
        });
    });
}

var app = builder.Build();

if (allowedOrigins is { Length: > 0 })
{
    app.UseCors();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/info", () =>
{
    var appVersion = Environment.GetEnvironmentVariable("APP_VERSION");
    if (string.IsNullOrWhiteSpace(appVersion))
    {
        appVersion = "dev";
    }

    var containerHostname = Environment.GetEnvironmentVariable("HOSTNAME");
    if (string.IsNullOrWhiteSpace(containerHostname))
    {
        containerHostname = Environment.MachineName;
    }

    return Results.Ok(new
    {
        machineName = Environment.MachineName,
        hostName = Dns.GetHostName(),
        containerHostname,
        environment = app.Environment.EnvironmentName,
        appVersion,
        utc = DateTime.UtcNow,
    });
});

string[] summaries =
[
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching",
];

app.MapGet("/api/weatherforecast", () =>
{
    return Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]));
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
