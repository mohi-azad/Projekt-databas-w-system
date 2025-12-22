namespace Projekt_databas_och_w_system.Models.Details
{
    public class MoveDetails
    {
        public int MoveId {  get; set; }
        public int GameId {  get; set; }
        public int PlayerId { get; set; }
        public int BoxId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
