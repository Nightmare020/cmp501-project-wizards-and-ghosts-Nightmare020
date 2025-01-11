using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MatchmakingHub : Hub
{
    private static readonly List<PlayerInfo> matchmakingPool = new List<PlayerInfo>();

    public async Task SelectRole(string role)
    {
        string connectionID = Context.ConnectionId;
        Console.WriteLine($"Player {connectionID} selected role {role}");

        // Add player to matchmaking pool
        matchmakingPool.Add(new PlayerInfo { ConnectionId = connectionID, Role = role });
        
        // try to find a match
        await TryMatchPlayers();
    }

    private async Task TryMatchPlayers()
    {
        // Find a wizard and a ghost in the pool
        PlayerInfo wizard = matchmakingPool.FirstOrDefault(p => p.Role == "Wizard");
        PlayerInfo ghost = matchmakingPool.FirstOrDefault(p => p.Role == "Ghost");

        if (wizard != null && ghost != null)
        {
            matchmakingPool.Remove(wizard);
            matchmakingPool.Remove(ghost);

            // Notify both players
            Console.WriteLine($"Matching {wizard.ConnectionId} as Wizard and {ghost.ConnectionId} as Ghost");
            await Clients.Client(wizard.ConnectionId).SendAsync("Matched", "Wizard");
            await Clients.Client(ghost.ConnectionId).SendAsync("Matched", "Ghost");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        matchmakingPool.RemoveAll(p => p.ConnectionId == Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

public class PlayerInfo
{
    public string ConnectionId { get; set; }
    public string Role { get; set; } // "Wizard" or "Ghost" role player
}

