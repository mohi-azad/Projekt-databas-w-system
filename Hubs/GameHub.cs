using Microsoft.AspNetCore.SignalR;

namespace Projekt_databas_och_w_system.Hubs
{
    public class GameHub: Hub
    {
        public Task JoinGameGroup(string gameId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        }

        public Task JoinPlayerGroup(string playerGroup)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, playerGroup);
        }
    }
}
