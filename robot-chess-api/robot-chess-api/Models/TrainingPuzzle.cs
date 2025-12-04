using System;
using System.Collections.Generic;

namespace robot_chess_api.Models;

public partial class TrainingPuzzle
{
    public Guid Id { get; set; }

    public string FenStr { get; set; } = null!;

    public string SolutionMove { get; set; } = null!;

    public string? Difficulty { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string Name { get; set; } = "Puzzle";

    public string? Description { get; set; }

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
