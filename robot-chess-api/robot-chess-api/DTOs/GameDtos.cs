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
        
        // Elo Rating tracking
        public int? PlayerRatingBefore { get; set; }
        public int? PlayerRatingAfter { get; set; }
        public int? RatingChange { get; set; }
        
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
        public Guid? PuzzleId { get; set; } // ID of the puzzle if training_puzzle mode
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

    // Request to update game result and status
    public class UpdateGameResultRequestDto
    {
        public Guid GameId { get; set; }
        public string Result { get; set; } = string.Empty; // "win", "lose", "draw"
        public string Status { get; set; } = "completed"; // "completed" (auto-mapped to "finished"), "abandoned", "waiting", "in_progress"
        public int? TotalMoves { get; set; }
        public string? FenCurrent { get; set; }
    }

    // Response after updating game result
    public class UpdateGameResultResponseDto
    {
        public Guid GameId { get; set; }
        public string Result { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? TotalMoves { get; set; }
        public DateTime? EndedAt { get; set; }
        public string Message { get; set; } = string.Empty;
        
        // Elo Rating information
        public int? PlayerRatingBefore { get; set; }
        public int? PlayerRatingAfter { get; set; }
        public int? RatingChange { get; set; }
    }

    // Request to end game
    public class EndGameRequestDto
    {
        public Guid GameId { get; set; }
        public string Reason { get; set; } = "user_ended"; // resign, timeout, abandoned, user_ended
    }

    // Response after ending game
    public class EndGameResponseDto
    {
        public Guid GameId { get; set; }
        public Guid RequestId { get; set; }
        public string Status { get; set; } = string.Empty; // sent, error
        public string Message { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    // DTO for game replay with full details
    public class GameReplayDto
    {
        public Guid GameId { get; set; }
        public Guid? PlayerId { get; set; }
        public string? PlayerName { get; set; }
        public string? Status { get; set; }
        public string? Result { get; set; }
        public string? Difficulty { get; set; }
        
        // Game timing
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? DurationSeconds { get; set; }
        
        // Board information
        public string? FenStart { get; set; }
        public string? FenCurrent { get; set; }
        public int? TotalMoves { get; set; }
        
        // Elo Rating
        public int? PlayerRatingBefore { get; set; }
        public int? PlayerRatingAfter { get; set; }
        public int? RatingChange { get; set; }
        
        // Game type
        public GameTypeDto? GameType { get; set; }
        
        // All moves for replay
        public List<GameMoveDto> Moves { get; set; } = new List<GameMoveDto>();
        
        // Statistics
        public GameStatisticsDto? Statistics { get; set; }
    }

    // DTO for game statistics
    public class GameStatisticsDto
    {
        public int TotalMoves { get; set; }
        public int WhiteMoves { get; set; }
        public int BlackMoves { get; set; }
        public int Captures { get; set; }
        public int Checks { get; set; }
        public double AverageMoveTimeSeconds { get; set; }
        public string? LongestThinkingMove { get; set; }
        public double LongestThinkingTimeSeconds { get; set; }
    }
}
