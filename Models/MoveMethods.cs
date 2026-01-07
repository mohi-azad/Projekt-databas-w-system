using Microsoft.Data.SqlClient;
using Projekt_databas_och_w_system.Models.Details;

public class MoveMethods
{
    // metod för att logga drag i ett spel
    public void AddMove(SqlConnection connection, int gameId, int playerId, int boxId)
    {
        // skapar och sparar vilket spel, spelare och box
        string sql = @"INSERT INTO Moves (GameId, PlayerId, BoxId, CreatedAt) VALUES (@game, @player, @box, GETDATE())";

        using SqlCommand cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@game", gameId);
        cmd.Parameters.AddWithValue("@player", playerId);
        cmd.Parameters.AddWithValue("@box", boxId);
        cmd.ExecuteNonQuery();
    }


    // metod för att hämta alla drag för ett spel
    public List<MoveDetails> GetMoves(SqlConnection connection, int gameId)
    {
        List<MoveDetails> moves = new List<MoveDetails>();
        string sql = "SELECT MoveId, PlayerId, BoxId, MoveTime FROM Moves WHERE GameId=@game ORDER BY MoveTime";
        using SqlCommand cmd = new SqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@game", gameId);
        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            moves.Add(new MoveDetails
            {
                MoveId = reader.GetInt32(0),
                PlayerId = reader.GetInt32(1),
                BoxId = reader.GetInt32(2),
                CreatedAt = reader.GetDateTime(3)
            });
        }
        return moves;
    }
}
