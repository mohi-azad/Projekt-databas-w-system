using System.Reflection.Metadata.Ecma335;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Projekt_databas_och_w_system.Models;
namespace Projekt_databas_och_w_system.Controllers
{
    public class LoginController : Controller
    {
        private readonly PlayerMethods _playerMethods = new();
        private string ConnectionString =>
    "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=HiddenGold;Integrated Security=True;";


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            using SqlConnection sqlConnection = new(ConnectionString);
            
           
            // öppnar databasen
            sqlConnection.Open();
            {
                // kontrollerar inloggningen mha SQL-satser
                string sql = "SELECT PlayerId FROM Players WHERE PlayerName=@username AND PasswordHash=@password";
                SqlCommand command= new SqlCommand(sql, sqlConnection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                var reader = command.ExecuteReader();
                
                // kollar om användaren finns
                if (!reader.Read())
                {
                    ViewBag.Error = "Fel användarnamn eller lösenord";
                    return View();
                }
                int playerId = (int)reader["PlayerId"];
                // hämtar playerId och lagrar det i session
                HttpContext.Session.SetInt32("PlayerId", playerId);
                return RedirectToAction("Lobby", "Game");
            }
        }
        
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // metod för att kontrollera regisreringen 
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "All fields are required";
                return View();
            }

            var success = _playerMethods.RegisterPlayer(username, password);

            if (!success)
            {
                ViewBag.Error = "Username already exists";
                return View();
            }

            return RedirectToAction("Login", "Login");
        }

    }
}
