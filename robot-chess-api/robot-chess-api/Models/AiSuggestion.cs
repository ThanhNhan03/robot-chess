using System;
using System.Collections.Generic;

namespace robot_chess_api.Models;

public partial class AiSuggestion
{
    public Guid Id { get; set; }

    public Guid? GameId { get; set; }

    public Guid? MoveId { get; set; }

    public Guid? UserId { get; set; }

    public string? SuggestedMove { get; set; }

    public string? FenPosition { get; set; }

    public int? Evaluation { get; set; }

    public decimal? Confidence { get; set; }

    public int? PointsDeducted { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual GameMove? Move { get; set; }

    public virtual User? User { get; set; }
}

