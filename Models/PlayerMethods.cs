using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Projekt_databas_och_w_system.Models.Details;
using System.Security.Cryptography;
using System.Text;

public class PlayerMethods
{
    // skapar en ny spelare
    public int CreatePlayer(SqlConnection connection, string playerName)
    {
        string sql = "INSERT INTO Players (PlayerName) VALUES (@name); SELECT SCOPE_IDENTITY();";
        using SqlCommand cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@name", playerName);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    // hämtar och läser en spelare baserad på playerId
    public PlayerDetails GetPlayer(SqlConnection connection, int playerId)
    {
        string sql = "SELECT PlayerId, PlayerName FROM Players WHERE PlayerId=@id";
        using SqlCommand cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", playerId);

        using SqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new PlayerDetails
            {
                PlayerId = reader.GetInt32(0),
                PlayerName = reader.GetString(1)
            };
        }
        return null;
    }

    // metod för att lägga till nya spelare
    public bool RegisterPlayer(string username, string password)
    {
        SqlConnection sqlConnection = new SqlConnection();
        // Skapa koppling mot lokal instans av databas
        sqlConnection.ConnectionString =
            "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=HiddenGold;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
        // kontrollera om spelaren är redan registrerad
        sqlConnection.Open();
        string checksql = "SELECT COUNT(*) FROM Players WHERE PlayerName= @n";
        SqlCommand cmd = new SqlCommand(checksql, sqlConnection);
        cmd.Parameters.AddWithValue("@n", username);

        // kolla detta mha scalar
        if ((int)cmd.ExecuteScalar() > 0)
        {
            return false;
        }

        // skalpa hashfunktion för att lägga till
        string hash = HashPassword(password);
        string insertsql = @"INSERT INTO Players(PlayerName, PasswordHash) VALUES(@n, @p)";
        SqlCommand insertcmd = new SqlCommand(insertsql, sqlConnection);
        insertcmd.Parameters.AddWithValue("@n", username);
        insertcmd.Parameters.AddWithValue("@p", hash);
        insertcmd.ExecuteNonQuery();
        return true;
    }


    // privatfunktion för att sätta in lösenord
    public string HashPassword(string password)
    {
        using SHA256 sha = SHA256.Create();
        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }




}
