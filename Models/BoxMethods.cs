using Microsoft.Data.SqlClient;
using Projekt_databas_och_w_system.Models.Details;

namespace Projekt_databas_och_w_system.Models
{
    /**
     * Klassen BoxMethods
     * ansvarar för all databaslogik kopplad till boxar i ett spel.
     * används av GameMethods för att skapa boxar och innehåller ingen spellogik.
     **/
    public class BoxMethods
    {
        // metod för att skapa boxar för ett nytt spel
        public void CreateBoxes(SqlConnection connection, int gameId, int boxCount, int bombCount)
        {
            // slumpa guldbiten bland boxarna
            Random rng = new Random();
            int goldIndex = rng.Next(boxCount);

            // hashset används för att undvika dubbla bomb-positioner
            // antal bomber baserad på boxar
            HashSet<int> bombs = new HashSet<int>();
            // lägger till antal bomber slumpmässigt (inte i boxen som innehåller guld)
            while(bombs.Count < bombCount)
            {
                int b= rng.Next(boxCount);
                if(b != goldIndex)
                {
                    bombs.Add(b);
                }
            }

            // lägger objekt i boxarna
            // skapa varje box och sparar dem i databasen
            for(int i= 0; i < boxCount; i++)
            {
                string sqlstring = @"INSERT INTO Boxes (GameId, PositionIndex, IsBomb, IsGold) VALUES (@g, @i, @bomb, @gold)";

                using SqlCommand cmd = new SqlCommand(sqlstring, connection);
                cmd.Parameters.AddWithValue("@g", gameId);
                cmd.Parameters.AddWithValue("@i", i);
                cmd.Parameters.AddWithValue("@bomb", bombs.Contains(i));
                cmd.Parameters.AddWithValue("@gold", i== goldIndex);
                cmd.ExecuteNonQuery();
            }  
        }

        // metod för att hämta och läsa alla boxar för ett specifik spel
        public List<BoxDetails> GetBoxes(SqlConnection connection, int gameId)
        {
            List<BoxDetails> boxes = new List<BoxDetails>();
            string sqlstring = @"SELECT BoxId, PositionIndex, IsOpen, IsBomb, IsGold FROM Boxes WHERE GameId=@g ORDER BY PositionIndex";

            using SqlCommand cmd = new SqlCommand(sqlstring, connection);
            cmd.Parameters.AddWithValue("@g", gameId);

            // läser boxarna från databasen och skapar BoxDetails-objekt
            using SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                // skapa boxobjekt baserad på databasvärden
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
        // öppnar en box och markerar den som öppnad i databasen
        public BoxDetails OpenBox(SqlConnection connection, int boxId)
        {
            BoxDetails box = null;
            string sqlstring = "SELECT IsOpen, IsBomb, IsGold, PositionIndex, GameId FROM Boxes WHERE BoxId=@b";
            
            using SqlCommand cmd = new SqlCommand(sqlstring, connection);
            cmd.Parameters.AddWithValue("@b", boxId);

            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                // säkerhetskontroll dvs att boxen inte får öppnas två gånger
                if ((bool)reader["IsOpen"])
                    throw new Exception("Box is already opened!");
                // skapa ett objekt av box baserad på databasen
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

            // Markera boxen som öppnad i databasen
            // och uppdatera boxen till öppnad
            string updateSql = "UPDATE Boxes SET IsOpen=1 WHERE BoxId=@b";
            using SqlCommand updateCmd = new SqlCommand(updateSql, connection);
            updateCmd.Parameters.AddWithValue("@b", boxId);
            updateCmd.ExecuteNonQuery();
            box.IsOpen = true;
            return box;
        }
    }
}
