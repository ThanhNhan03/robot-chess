using System;
using System.Collections.Generic;

namespace robot_chest_api.Data;

public partial class SavedState
{
    public Guid Id { get; set; }

    public Guid? GameId { get; set; }

    public Guid? PlayerId { get; set; }

    public string FenStr { get; set; } = null!;

    public Guid? LastMoveId { get; set; }

    public DateTime? SavedAt { get; set; }

    public virtual GameMove? LastMove { get; set; }

    public virtual User1? Player { get; set; }
}
