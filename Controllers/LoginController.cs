using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
namespace Projekt_databas_och_w_system.Controllers
{
    public class LoginController : Controller
    {
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
    }
}
