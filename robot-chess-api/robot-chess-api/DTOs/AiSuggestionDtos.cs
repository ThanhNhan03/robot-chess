using System;
using System.ComponentModel.DataAnnotations;

namespace robot_chess_api.DTOs
{
    /// <summary>
    /// Request DTO for getting AI chess move suggestion
    /// </summary>
    public class GetSuggestionRequestDto
    {
        [Required]
        public Guid GameId { get; set; }

        /// <summary>
        /// Current board position in FEN notation
        /// Example: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        /// </summary>
        [Required]
        public string FenPosition { get; set; } = string.Empty;

        /// <summary>
        /// Analysis depth (higher = better but slower)
        /// Range: 10-20 recommended
        /// </summary>
        public int Depth { get; set; } = 15;
    }

    /// <summary>
    /// Response DTO for AI suggestion
    /// </summary>
    public class SuggestionResponseDto
    {
        public Guid SuggestionId { get; set; }
        
        /// <summary>
        /// Suggested move in UCI format (e.g., "e2e4", "e7e5")
        /// </summary>
        public string SuggestedMove { get; set; } = string.Empty;

        /// <summary>
        /// Suggested move in SAN format (e.g., "Nf3", "e4")
        /// </summary>
        public string SuggestedMoveSan { get; set; } = string.Empty;

        /// <summary>
        /// Evaluation score in centipawns (positive = white advantage)
        /// </summary>
        public int? Evaluation { get; set; }

        /// <summary>
        /// Confidence level (0.0 - 1.0)
        /// </summary>
        public decimal Confidence { get; set; }

        /// <summary>
        /// Best line continuation
        /// </summary>
        public List<string> BestLine { get; set; } = new List<string>();

        /// <summary>
        /// Points deducted for this suggestion
        /// </summary>
        public int PointsDeducted { get; set; }

        /// <summary>
        /// User's remaining points balance
        /// </summary>
        public int RemainingPoints { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for suggestion history item
    /// </summary>
    public class SuggestionHistoryDto
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public string? GameType { get; set; }
        public string SuggestedMove { get; set; } = string.Empty;
        public string? FenPosition { get; set; }
        public decimal Confidence { get; set; }
        public int PointsDeducted { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Response for suggestion history list
    /// </summary>
    public class SuggestionHistoryResponseDto
    {
        public List<SuggestionHistoryDto> Suggestions { get; set; } = new List<SuggestionHistoryDto>();
        public int TotalCount { get; set; }
        public int TotalPointsSpent { get; set; }
    }
}
