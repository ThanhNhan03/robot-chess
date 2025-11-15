using System;
using System.Collections.Generic;

namespace robot_chest_api.Models;

public partial class Game
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

    public virtual GameType? GameType { get; set; }

    public virtual AppUser? Player { get; set; }

    public virtual TrainingPuzzle? Puzzle { get; set; }
}
