using System;
using System.Collections.Generic;

namespace robot_chess_api.Models;

public partial class GameMove
{
    public Guid Id { get; set; }

    public Guid? GameId { get; set; }

    public int? MoveNumber { get; set; }

    public string? PlayerColor { get; set; }

    public string? FromSquare { get; set; }

    public string? ToSquare { get; set; }

    public string? FromPiece { get; set; }

    public string? ToPiece { get; set; }

    public string? Notation { get; set; }

    public bool? ResultsInCheck { get; set; }

    public string? FenStr { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<AiSuggestion> AiSuggestions { get; set; } = new List<AiSuggestion>();

    public virtual ICollection<SavedState> SavedStates { get; set; } = new List<SavedState>();
}
