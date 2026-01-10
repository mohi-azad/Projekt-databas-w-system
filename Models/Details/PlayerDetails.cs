namespace Projekt_databas_och_w_system.Models.Details
{
    public class PlayerDetails
    {
        public int PlayerId { get;set; }
        public string PlayerName {  get;set; }
       
        public string PasswordHash { get;set; }
        public DateTime CreatedAt {  get;set; }
        public ICollection<GameDetails> CreatedGames { get;set; }
        public ICollection<GameDetails> Player1Games { get;set; }
        public ICollection<GameDetails> Player2Games { get;set; }

    }
}
