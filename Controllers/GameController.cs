using Microsoft.AspNetCore.Mvc;
using Projekt_databas_och_w_system.Models;
using Projekt_databas_och_w_system.Models.Details;

namespace Projekt_databas_och_w_system.Controllers
{
    public class GameController : Controller
    {
        // Instanser av metoder
        private readonly GameMethods _gameMethods = new();
        private readonly MoveMethods _moveMethods = new();

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
        public IActionResult JoinGame(int gameId)
        {
            int? playerId = HttpContext.Session.GetInt32("PlayerId");
            if (playerId == null) return RedirectToAction("Login", "Login");

            bool joined = _gameMethods.JoinGame(gameId, playerId.Value);
            if (!joined)
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
        public IActionResult OpenBox(int gameId, int boxId)
        {
            int? playerId = HttpContext.Session.GetInt32("PlayerId");
            if (playerId == null) return RedirectToAction("Login", "Login");

            try
            {
                _gameMethods.PlayerOpenBox(gameId, playerId.Value, boxId);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction("Play", new { id = gameId });
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
