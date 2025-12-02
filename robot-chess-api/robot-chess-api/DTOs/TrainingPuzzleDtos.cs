namespace robot_chess_api.DTOs
{
    public class TrainingPuzzleDto
    {
        public Guid Id { get; set; }
        public string FenStr { get; set; } = null!;
        public string SolutionMove { get; set; } = null!;
        public string? Difficulty { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
