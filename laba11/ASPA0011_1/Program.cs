using ASPA0011_1.Logging;
using ASPA0011_1.Services;
using Microsoft.Extensions.Logging.Console;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();

builder.Logging.AddConsole();
builder.Logging.AddDebug();
string logFilePath = $"logs/aspa-{DateTime.Now:yyyy-MM-dd}.log";
builder.Logging.AddProvider(new FileLoggerProvider(logFilePath));

builder.Logging.AddFilter((provider, category, logLevel) =>
{
    if (category.StartsWith("Microsoft"))
    {
        if (provider.Contains("ConsoleLoggerProvider"))
        {
            return logLevel >= LogLevel.Error;
        }
        return false;
    }

    if (provider.Contains("ConsoleLoggerProvider"))
    {
        return logLevel >= LogLevel.Warning;
    }

    return true;
});


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddSingleton<IChannelService, ChannelService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation(1, "Application starting up.");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.Use(async (context, next) =>
    {
        var requestLogger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        requestLogger.LogTrace("Request: {Method} {Path}", context.Request.Method, context.Request.Path);
        await next.Invoke();
        requestLogger.LogTrace("Response: {StatusCode}", context.Response.StatusCode);
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

AppDomain.CurrentDomain.ProcessExit += (s, e) =>
{
    logger.LogInformation(99, "Application shutting down.");
};

try
{
    app.Run();
}
catch (Exception ex)
{
    logger.LogCritical(100, ex, "Application terminated unexpectedly!");
}