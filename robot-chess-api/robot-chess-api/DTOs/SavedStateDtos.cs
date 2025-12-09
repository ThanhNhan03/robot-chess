namespace robot_chess_api.DTOs
{
    public class SaveGameStateRequestDto
    {
        public Guid GameId { get; set; }
        public string FenStr { get; set; } = string.Empty;
        public Guid? LastMoveId { get; set; }
    }

    public class SaveGameStateResponseDto
    {
        public Guid SavedStateId { get; set; }
        public Guid GameId { get; set; }
        public string FenStr { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ResumeGameResponseDto
    {
        public Guid GameId { get; set; }
        public Guid RequestId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string FenStr { get; set; } = string.Empty;
        public Guid? LastMoveId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? SavedAt { get; set; }
    }

    public class PauseGameResponseDto
    {
        public Guid GameId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid SavedStateId { get; set; }
    }
}
