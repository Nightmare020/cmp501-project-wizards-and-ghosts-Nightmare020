using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Player
{
    public string ConnectionId { get; set; }
    public string Role { get; set; } // "Wizard" or "Ghost" role player
}
