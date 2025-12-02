namespace robot_chess_api.DTOs
{
    // DTOs for Game Type
    public class GameTypeDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    // DTOs for Game
    public class GameDto
    {
        public Guid Id { get; set; }
        public Guid? PlayerId { get; set; }
        public string? Status { get; set; }
        public string? Result { get; set; }
        public string? FenStart { get; set; }
        public string? FenCurrent { get; set; }
        public int? TotalMoves { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? GameTypeId { get; set; }
        public Guid? PuzzleId { get; set; }
        public string? Difficulty { get; set; }
        
        // Navigation properties
        public GameTypeDto? GameType { get; set; }
        public string? PlayerName { get; set; }
    }

    // Request to start a new game
    public class StartGameRequestDto
    {
        public string GameTypeCode { get; set; } = "normal_game"; // normal_game or training_puzzle
        public string Difficulty { get; set; } = "medium"; // easy, medium, hard
        public Guid? PuzzleId { get; set; } // Required if game_type is training_puzzle
    }

    // Response after starting a game
    public class StartGameResponseDto
    {
        public Guid GameId { get; set; }
        public Guid RequestId { get; set; }
        public string GameTypeCode { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? FenStart { get; set; }
    }

    // Request to resume a game
    public class ResumeGameRequestDto
    {
        public Guid GameId { get; set; }
    }

    // Request to verify board setup
    public class VerifyBoardSetupRequestDto
    {
        public Guid GameId { get; set; }
    }

    // Response for board setup verification
    public class BoardSetupStatusDto
    {
        public Guid GameId { get; set; }
        public string Status { get; set; } = string.Empty; // "correct" or "incorrect"
        public string? Expected { get; set; }
        public string? Detected { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    // Request to test training puzzle (for development/testing only)
    public class TestPuzzleRequestDto
    {
        public string PuzzleFen { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "medium";
    }
}
