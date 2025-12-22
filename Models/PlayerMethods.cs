using Microsoft.Data.SqlClient;
using Projekt_databas_och_w_system.Models.Details;

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
}
