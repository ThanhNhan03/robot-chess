using System;
using System.Collections.Generic;

namespace robot_chest_api.Data;

public partial class AiSuggestion
{
    public Guid Id { get; set; }

    public Guid? GameId { get; set; }

    public Guid? MoveId { get; set; }

    public string? SuggestedMove { get; set; }

    public decimal? Confidence { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual GameMove? Move { get; set; }
}
