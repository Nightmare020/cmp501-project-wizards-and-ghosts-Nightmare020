using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class MatchmakingHub : Hub
{
    private static List<Player> matchmakingPool = new List<Player>();

    public async Task SelectRole(string role)
    {
        var player = new Player
        {
            ConnectionId = Context.ConnectionId,
            Role = role
        };

        matchmakingPool.Add(player);
        await TryMatchPlayers();
    }

    private async Task TryMatchPlayers()
    {
        var wizard = matchmakingPool.FirstOrDefault(p => p.Role == "Wizard");
        var ghost = matchmakingPool.FirstOrDefault(p => p.Role == "Ghost");

        if (wizard != null && ghost != null)
        {
            matchmakingPool.Remove(wizard);
            matchmakingPool.Remove(ghost);

            // Notify both players
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
