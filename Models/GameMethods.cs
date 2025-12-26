using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.VisualBasic;
using Projekt_databas_och_w_system.Models.Details;

namespace Projekt_databas_och_w_system.Models
{
    public class GameMethods
    {
        private readonly BoxMethods _boxMethods = new();
        private readonly MoveMethods _moveMethods = new();
        private string ConnectionString =>
    "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=HiddenGold;Integrated Security=True;";


        // metod för att skapa ett spel
        public int CreateGame(int playerId)
        {
            using SqlConnection sqlConnection = new (ConnectionString);
            sqlConnection.Open();
            string gameSql = @"
                    INSERT INTO Games (CreatedByPlayerId, Player1Id, Player2Id, CurrentTurnPlayerId)
                    VALUES (@p, @p, NULL, @p);
                    SELECT SCOPE_IDENTITY();";

            using SqlCommand cmd = new SqlCommand(gameSql, sqlConnection);
            cmd.Parameters.AddWithValue("@p", playerId);

            int gameId = Convert.ToInt32(cmd.ExecuteScalar());
            // Skapa boxarna mha Boxmethods
            _boxMethods.CreateBoxes(sqlConnection, gameId);
            return gameId;
        }


        // metod för att hämta spelstatus och boxar från BoxMethods
        public (List<BoxDetails> boxes, int currentTurn, bool isFinished, int? winnerId, int? player2Id) GetGameState(int gameId)
        {
            int currentTurn = 0;
            bool isFinished = false;
            int? winnerId = null;
            int? player2Id = null;

            using SqlConnection sqlConnection = new(ConnectionString);
            sqlConnection.Open();
            string gameSql = @"
                    SELECT CurrentTurnPlayerId, IsFinished, WinnerPlayerId, Player2Id
                    FROM Games WHERE GameId=@id";

            using (SqlCommand cmd = new SqlCommand(gameSql, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@id", gameId);

                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    currentTurn = reader.GetInt32(0);
                    isFinished = reader.GetBoolean(1);
                    if (!reader.IsDBNull(2)) winnerId = reader.GetInt32(2);
                    if (!reader.IsDBNull(3)) player2Id = reader.GetInt32(3);
                }

            }            
            // Hämta boxarna via BoxMethods
            List<BoxDetails> boxes = _boxMethods.GetBoxes(sqlConnection, gameId);
            return (boxes, currentTurn, isFinished, winnerId, player2Id);
        }


        // metod för att gå med i ett spel
        public bool JoinGame(int gameId, int playerId)
        {
            using SqlConnection sqlConnection = new(ConnectionString);
            sqlConnection.Open();

            // kollar om player2 har gått med i spelet
            string checkSql = "SELECT Player2Id FROM Games WHERE GameId=@g";
            using SqlCommand checkCmd = new SqlCommand(checkSql, sqlConnection);
            checkCmd.Parameters.AddWithValue("@g", gameId);

            object result = checkCmd.ExecuteScalar();
            if (result != DBNull.Value) return false;
           
            string updateSql = "UPDATE Games SET Player2Id=@p WHERE GameId=@g";
            using SqlCommand updateCmd = new SqlCommand(updateSql, sqlConnection);
            updateCmd.Parameters.AddWithValue("@p", playerId);
            updateCmd.Parameters.AddWithValue("@g", gameId);
            updateCmd.ExecuteNonQuery();
            return true;
        }


        // metod för att ta bort ett spel från spellista
        public void DeleteGame(int gameId)
        {
            using SqlConnection sqlConnection = new(ConnectionString);
            sqlConnection.Open();

            string deleteMoves = "DELETE FROM Moves WHERE GameId=@g";
            using (SqlCommand cmd = new SqlCommand(deleteMoves, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@g", gameId);
                cmd.ExecuteNonQuery();
            }

            string deleteBoxes = "DELETE FROM Boxes WHERE GameId=@g";
            using (SqlCommand cmd = new SqlCommand(deleteBoxes, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@g", gameId);
                cmd.ExecuteNonQuery();
            }

            string deleteGame = "DELETE FROM Games WHERE GameId=@g";
            using (SqlCommand cmd = new SqlCommand(deleteGame, sqlConnection))
            {
                cmd.Parameters.AddWithValue("@g", gameId);
                cmd.ExecuteNonQuery();
            }
        }

        // metod för att kolla om spelet är färdigt
        public bool IsGameFinished(int gameId)
        {
            using SqlConnection sqlConnection = new(ConnectionString);
            sqlConnection.Open();
            string sqlstring = "SELECT COUNT(*) FROM Boxes WHERE GameId=@g AND IsGold=1 AND IsOpen=1";
            SqlCommand cmd = new SqlCommand(sqlstring, sqlConnection);
            cmd.Parameters.AddWithValue("g", gameId);

            bool hasWinner = (int)cmd.ExecuteScalar() > 0;
            if (hasWinner)
            {
                string sqlupdate = "UPDATE Games SET IsFinished=1 WHERE GameId= @g";
                SqlCommand updatecmd = new SqlCommand(sqlupdate, sqlConnection);
                updatecmd.Parameters.AddWithValue("@g", gameId);
                updatecmd.ExecuteNonQuery();
            }
            return hasWinner;
        }

        // metood för att skapa en lista av spel
        public List<GameLobby> GetLobbyGames()
        {
            List<GameLobby> games = new();

            using SqlConnection sqlConnection = new(ConnectionString);
            sqlConnection.Open();

            string sql = @"
                SELECT g.GameId, g.Player1Id, g.Player2Id, g.IsFinished,
                       p1.PlayerName, p2.PlayerName
                FROM Games g
                JOIN Players p1 ON g.Player1Id = p1.PlayerId
                LEFT JOIN Players p2 ON g.Player2Id = p2.PlayerId";

            using SqlCommand cmd = new(sql, sqlConnection);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                games.Add(new GameLobby
                {
                    GameId = reader.GetInt32(0),
                    Player1Id = reader.GetInt32(1),
                    Player2Id = reader.IsDBNull(2) ? null : reader.GetInt32(2),
                    IsFinished = reader.GetBoolean(3),
                    Player1Name = reader.GetString(4),
                    Player2Name = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }
            return games;
        }


        // metod för spelarens drag i spel
        public BoxResult PlayerOpenBox(int gameId, int playerId, int boxId)
        {
            
            using SqlConnection sqlConnection = new(ConnectionString);
            sqlConnection.Open();
            // kontrollera vems tur är det i spelet
            string turnchecksql = @"SELECT CurrentTurnPlayerId, IsFinished, player2Id FROM Games WHERE GameId= @g";

            using(SqlCommand checkcmd= new SqlCommand(turnchecksql, sqlConnection))
            {
                checkcmd.Parameters.AddWithValue("@g", gameId);
                using var reader= checkcmd.ExecuteReader();
                if (!reader.Read())
                    throw new Exception("Game not found");

                bool isFinished= reader.GetBoolean(1);
                int currentTurnPlayerId= reader.GetInt32(0);

                if (isFinished)
                    throw new Exception("Game is already finished");

                if (currentTurnPlayerId != playerId)
                    throw new Exception("It is not your turn!");

                if (reader.IsDBNull(2))
                {
                    throw new Exception("Waiting for second player!");
                }
            }

            // öppna box
            BoxDetails box = _boxMethods.OpenBox(sqlConnection, boxId);
            // göra ett drag
            _moveMethods.AddMove(sqlConnection, gameId, playerId, boxId);

            // hämta aktuell tur, nästa spelare och extra tur
            int currentTurn = 0;
            int nextTurn = 0;
            int extraTurns = 0;

            string turnSql = "SELECT CurrentTurnPlayerId, Player1Id, Player2Id, ExtraTurns FROM Games WHERE GameId=@g";
            using (SqlCommand turnCmd = new SqlCommand(turnSql, sqlConnection))
            {
                turnCmd.Parameters.AddWithValue("@g", gameId);
                using (var reader = turnCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        currentTurn = (int)reader["CurrentTurnPlayerId"];
                        int player1 = (int)reader["Player1Id"];
                        int player2 = (int)reader["Player2Id"];
                        extraTurns = reader["ExtraTurns"] != DBNull.Value ? (int)reader["ExtraTurns"] : 0;

                        // Beräkna nästa spelare
                        nextTurn = (currentTurn == player1) ? player2 : player1;
                    }
                }
            }

            // hantera vad som finns i en box
            if (box.IsGold)
            {
                string sqlgold = "UPDATE Games SET WinnerPlayerId=@p, IsFinished=1 WHERE GameId=@g";
                using SqlCommand goldcmd = new SqlCommand(sqlgold, sqlConnection);
                goldcmd.Parameters.AddWithValue("@p", playerId);
                goldcmd.Parameters.AddWithValue("@g", gameId);
                goldcmd.ExecuteNonQuery();
                return BoxResult.Gold;
            }
            if (box.IsGold)
            {
                string sqlbomb = "UPDATE Games SET CurrentTurnPlayerId=@next WHERE GameId=@g";
                using SqlCommand bombcmd = new SqlCommand(sqlbomb, sqlConnection);
                bombcmd.Parameters.AddWithValue("@next", nextTurn);
                bombcmd.Parameters.AddWithValue("@g", gameId);
                bombcmd.ExecuteNonQuery();
                return BoxResult.Bomb;
            }
            if(extraTurns > 0)
            {
                string sqlextra = "UPDATE Games SET ExtraTurns = ExtraTurns -1 WHERE GameId=@g";
                using SqlCommand extracmd = new SqlCommand(sqlextra, sqlConnection);
                extracmd.Parameters.AddWithValue("@g", gameId);
                extracmd.ExecuteNonQuery();
                return BoxResult.Empty;
            }

            string sql = "UPDATE Games SET CurrentTurnPlayerId=@next WHERE GameId=@g";
            using SqlCommand cmd = new SqlCommand(sql, sqlConnection);
            cmd.Parameters.AddWithValue("@next", nextTurn);
            cmd.Parameters.AddWithValue("@g", gameId);
            cmd.ExecuteNonQuery();
            return BoxResult.Empty;
        }
    }

}
