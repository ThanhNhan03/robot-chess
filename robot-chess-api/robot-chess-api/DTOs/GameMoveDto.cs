namespace robot_chess_api.DTOs
{
    /// <summary>
    /// DTO for creating a new game move
    /// </summary>
    public class CreateGameMoveDto
    {
        public Guid GameId { get; set; }
        public int MoveNumber { get; set; }
        public string PlayerColor { get; set; } = string.Empty; // "white" or "black"
        public string FromSquare { get; set; } = string.Empty; // e.g., "e2"
        public string ToSquare { get; set; } = string.Empty; // e.g., "e4"
        public string? FromPiece { get; set; } // e.g., "white_pawn"
        public string? ToPiece { get; set; } // Captured piece, if any
        public string Notation { get; set; } = string.Empty; // e.g., "e4", "Nf3", "O-O"
        public bool ResultsInCheck { get; set; }
        public string FenStr { get; set; } = string.Empty; // Board state after move
    }

    /// <summary>
    /// DTO for returning game move information
    /// </summary>
    public class GameMoveDto
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public int MoveNumber { get; set; }
        public string PlayerColor { get; set; } = string.Empty;
        public string FromSquare { get; set; } = string.Empty;
        public string ToSquare { get; set; } = string.Empty;
        public string? FromPiece { get; set; }
        public string? ToPiece { get; set; }
        public string Notation { get; set; } = string.Empty;
        public bool ResultsInCheck { get; set; }
        public string FenStr { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for batch saving moves (useful for saving multiple moves at once)
    /// </summary>
    public class SaveMovesRequestDto
    {
        public Guid GameId { get; set; }
        public List<CreateGameMoveDto> Moves { get; set; } = new();
    }

    /// <summary>
    /// DTO for getting move history
    /// </summary>
    public class GetMovesRequestDto
    {
        public Guid GameId { get; set; }
        public int? FromMoveNumber { get; set; }
        public int? ToMoveNumber { get; set; }
    }
}
