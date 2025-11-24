namespace robot_chess_api.DTOs
{
    public class GameDto
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string FenCurrent { get; set; } = string.Empty;
        public int TotalMoves { get; set; }
        public DateTime? StartedAt { get; set; }
        public string Difficulty { get; set; } = string.Empty;
    }
}
