using System;
using System.Collections.Generic;

namespace robot_chest_api.Models;

public partial class TrainingPuzzle
{
    public Guid Id { get; set; }

    public string FenStr { get; set; } = null!;

    public string SolutionMove { get; set; } = null!;

    public string? Difficulty { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
