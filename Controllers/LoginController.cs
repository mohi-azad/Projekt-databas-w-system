using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace Projekt_databas_och_w_system.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            // skapa en kopling mot lokal instans av databasen
            SqlConnection sqlConnection = new SqlConnection();
            sqlConnection.ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=HiddenGold;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

            // öppnar databasen
            sqlConnection.Open();
            {
                // kontrollerar inloggningen mha SQL-satser
                string sql = "SELECT * FROM Players WHERE PlayerName=@username AND PasswordHash=@password";
                SqlCommand command= new SqlCommand(sql, sqlConnection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                var reader= command.ExecuteReader();
                
                // kollar om användaren finns
                if (!reader.Read())
                {
                    ViewBag.Error = "Incrrect username or password";
                    return View();
                }
                // hämtar playerId och lagrar det i session
                int playerId = (int)reader["PlayerId"];
                HttpContext.Session.SetInt32("PlayerId", playerId);
                return RedirectToAction("PlayerId", playerId);
            }
        }       
    }
}
