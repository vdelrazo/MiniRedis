using MiniRedis.Services;
using MiniRedis.Stores;
using MiniRedis.Utils;

var builder = WebApplication.CreateBuilder(args);

// Register the in-memory store and Redis-like service as singletons
builder.Services.AddSingleton<InMemoryDataStore>();
builder.Services.AddSingleton<IRedisService, RedisService>();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Global error handler for unhandled exceptions
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] {ex.Message}");
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\":\"Internal Server Error\"}");
    }
});

// Route incoming requests to controllers
app.MapControllers();

// Handle 404 errors with custom response
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\":\"Not Found\"}");
    }
});

// Run the HTTP server
app.Run("http://0.0.0.0:8080");

// Partial class required for integration testing support
public partial class Program { }
