namespace Projekt_databas_och_w_system.Models
{
    /*
     * GameLobby
     * definiera egenskaper och attribut denna klass ska ha
     * anropas och används av klassen GameMethods
     */
    public class GameLobby
    {
        public int GameId { get; set; }
        public int Player1Id { get; set; }
        public int? Player2Id { get; set; }
        public string Player1Name { get; set; }
        public string? Player2Name { get; set; }
        public bool IsFinished { get; set; }
    }
}
