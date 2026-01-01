using Microsoft.Data.SqlClient;
using Projekt_databas_och_w_system.Models.Details;

namespace Projekt_databas_och_w_system.Models
{
    public class BoxMethods
    {

        public void CreateBoxes(SqlConnection connection, int gameId)
        {
            Random rng = new Random();
            int goldIndex = rng.Next(20);
            HashSet<int> bombs = new HashSet<int>();
            
            // lägger till antal bomber
            while(bombs.Count < 7)
            {
                int b= rng.Next(20);
                if(b != goldIndex)
                {
                    bombs.Add(b);
                }
            }

            // lägger objekt i boxarna
            for(int i= 0; i < 20; i++)
            {
                string sqlstring = @"
                    INSERT INTO Boxes (GameId, PositionIndex, IsBomb, IsGold)
                    VALUES (@g, @i, @bomb, @gold)";

                using SqlCommand cmd = new SqlCommand(sqlstring, connection);
                cmd.Parameters.AddWithValue("@g", gameId);
                cmd.Parameters.AddWithValue("@i", i);
                cmd.Parameters.AddWithValue("@bomb", bombs.Contains(i));
                cmd.Parameters.AddWithValue("@gold", i== goldIndex);
                cmd.ExecuteNonQuery();
            }  
        }

        // metod för att hämta och läsa alla boxar
        public List<BoxDetails> GetBoxes(SqlConnection connection, int gameId)
        {
            List<BoxDetails> boxes = new List<BoxDetails>();
            string sqlstring = @"
                SELECT BoxId, PositionIndex, IsOpen, IsBomb, IsGold
                FROM Boxes
                WHERE GameId=@g
                ORDER BY PositionIndex";

            using SqlCommand cmd = new SqlCommand(sqlstring, connection);
            cmd.Parameters.AddWithValue("@g", gameId);

            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                boxes.Add(new BoxDetails
                {
                    BoxId = reader.GetInt32(0),
                    PositionIndex = reader.GetInt32(1),
                    IsOpen = reader.GetBoolean(2),
                    IsBomb = reader.GetBoolean(3),
                    IsGold = reader.GetBoolean(4)
                });
            }
            return boxes;
        }


        // metod för att hämta och läsa en enda box
        public BoxDetails OpenBox(SqlConnection connection, int boxId)
        {
            BoxDetails box = null;
            string sqlstring = "SELECT IsOpen, IsBomb, IsGold, PositionIndex, GameId FROM Boxes WHERE BoxId=@b";
            
            using SqlCommand cmd = new SqlCommand(sqlstring, connection);
            cmd.Parameters.AddWithValue("@b", boxId);

            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                if ((bool)reader["IsOpen"])
                    throw new Exception("Box is already opened!");

                box = new BoxDetails
                {
                    BoxId = boxId,
                    PositionIndex = (int)reader["PositionIndex"],
                    IsBomb = (bool)reader["IsBomb"],
                    IsGold = (bool)reader["IsGold"],
                    IsOpen = false
                };
            }
            reader.Close();

            // Markera som öppnad
            string updateSql = "UPDATE Boxes SET IsOpen=1 WHERE BoxId=@b";
            using SqlCommand updateCmd = new SqlCommand(updateSql, connection);
            updateCmd.Parameters.AddWithValue("@b", boxId);
            updateCmd.ExecuteNonQuery();

            box.IsOpen = true;
            return box;
        }
    }
}
