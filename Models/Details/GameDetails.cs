namespace Projekt_databas_och_w_system.Models.Details
{
    
    public class GameDetails
    {
        public int GameId {  get; set; }
        public int CreatedByPlayerId { get; set; }
        public int Player1Id { get; set; }
        public int Player2Id { get; set; }
        public int CurrentTurnPlayerId { get;set; }
        public DateTime CreatedAt {  get; set; }
        public int? WinnerPlayerId { get;set; }
        public bool IsFinished { get; set; }
        public bool ExtraTurns { get; set; }
        public string Player1Name {  get; set; }
        public string Player2Name { get; set; }
        public int BoxCount {  get; set; }
        public int BombCount { get; set; }
        public List<BoxDetails> Boxes { get; set; }
    }
    
}
