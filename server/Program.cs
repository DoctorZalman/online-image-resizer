using ImageResizer.Hubs;
using ImageResizer.Middleware;
using ImageResizer.Services;

var builder = WebApplication.CreateBuilder(args);

// - register controllers
builder.Services.AddControllers();

// - register SignalR for real-time progress updates
builder.Services.AddSignalR();

// - register app services in DI container
builder.Services.AddSingleton<UserQueueManager>();
builder.Services.AddSingleton<ImageResizeService>();

// - register background services
builder.Services.AddHostedService<ResizeWorker>();
builder.Services.AddHostedService<CleanupService>();

// - allow client origin with credentials for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "ClientPolicy",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:3000", "http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    );
});

// - increase max request size to 100MB for image uploads
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

var app = builder.Build();

app.UseCors("ClientPolicy");

// - session middleware must run before controllers and SignalR hub
app.UseMiddleware<SessionMiddleware>();

app.MapControllers();
app.MapHub<ResizeHub>("/hubs/resize");

app.Run();
