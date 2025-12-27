using Microsoft.AspNetCore.Mvc;
using Projekt_databas_och_w_system.Models;
using Projekt_databas_och_w_system.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Projekt_databas_och_w_system.Controllers
{
    public class GameController : Controller
    {
        // Instanser av metoder
        private readonly GameMethods _gameMethods = new();
        private readonly MoveMethods _moveMethods = new();
        private readonly IHubContext<GameHub> _hub;

        // metod för att skicka SignalR meddelanden
        public GameController(IHubContext<GameHub> hub)
        {
            _hub = hub;
        }
        // ----------------------------
        // LOBBY
        // ----------------------------
        public IActionResult Lobby()
        {
            int? playerId = HttpContext.Session.GetInt32("PlayerId");
            if (playerId == null) return RedirectToAction("Login", "Login");

            var games = _gameMethods.GetLobbyGames();
            return View(games);
        }

        // ----------------------------
        // CREATE GAME
        // ----------------------------
        public IActionResult CreateGame()
        {
            int? playerId = HttpContext.Session.GetInt32("PlayerId");
            if (playerId == null) return RedirectToAction("Login", "Login");

            int gameId = _gameMethods.CreateGame(playerId.Value);
            return RedirectToAction("Play", new { id = gameId });
        }

        // ----------------------------
        // JOIN GAME
        // ----------------------------
        [HttpPost]
        public async Task<IActionResult> JoinGame(int gameId)
        {
            int? playerId = HttpContext.Session.GetInt32("PlayerId");
            if (playerId == null) return RedirectToAction("Login", "Login");

            bool joined = _gameMethods.JoinGame(gameId, playerId.Value);
            if (joined)
            {
                await _hub.Clients
                    .Group($"game_{gameId}")
                    .SendAsync("playerJoined");
            }
            else
            {
                TempData["Error"] = "Another player already joined the game!";
            }
                return RedirectToAction("Play", new { id = gameId });
        }

        // ----------------------------
        // PLAY VIEW
        // ----------------------------
        public IActionResult Play(int id)
        {
            int? playerId = HttpContext.Session.GetInt32("PlayerId");
            if (playerId == null) return RedirectToAction("Login", "Login");

            var state = _gameMethods.GetGameState(id);

            ViewBag.GameId = id;
            ViewBag.Player1Name = state.player1Name;
            ViewBag.Player1Name = state.player1Name;
            ViewBag.CurrentTurn = state.currentTurn;
            ViewBag.IsFinished = state.isFinished;
            ViewBag.WinnerPlayerId = state.winnerId;
            ViewBag.Player2Id = state.player2Id;
            ViewBag.PlayerId = playerId.Value;
            



            return View(state.boxes);
        }

        // ----------------------------
        // OPEN BOX / MAKE MOVE
        // ----------------------------
        [HttpPost]
        public async Task<IActionResult> OpenBox(int gameId, int boxId)
        {
            int? playerId = HttpContext.Session.GetInt32("PlayerId");
            if (playerId == null) 
                return RedirectToAction("Login", "Login");

            try
            {
                BoxResult result = _gameMethods.PlayerOpenBox(gameId, playerId.Value, boxId);

                // meddela spelarna i spelet
                await _hub.Clients
                    .Group($"game_{gameId}")
                    .SendAsync("GameUpdated");

                if(result== BoxResult.Bomb)
                {
                    await _hub.Clients
                .Group($"player_{playerId.Value}")
                .SendAsync("BoxResult", "💣 BOOM! You hit a bomb!");
                }
                else if(result== BoxResult.Gold)
                {
                    await _hub.Clients
                    .Group($"player_{playerId.Value}")
                .SendAsync("BoxResult", "🏆 You found the gold!");
                }
            }

            catch (Exception ex)
            {
                await _hub.Clients
                    .Group($"player_{playerId.Value}")
                    .SendAsync("ErrorMessage", ex.Message);

            }
            return Ok();
        }

        // ----------------------------
        // DELETE GAME
        // ----------------------------
        [HttpPost]
        public IActionResult DeleteGame(int gameId)
        {
            _gameMethods.DeleteGame(gameId);
            return RedirectToAction("Lobby");
        }
    }
}
