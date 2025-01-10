using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add SignalR services
builder.Services.AddSignalR();

var app = builder.Build();

// Add a simple default route for testing
app.MapGet("/", () => "Server is running. Use /matchmaking for SignalR connections.");

// Map the signalR hub
app.MapHub<MatchmakingHub>("/matchmaking");

app.Run();
