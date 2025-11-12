using System;
using System.Collections.Generic;

namespace robot_chest_api.Data;

public partial class GameType
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();
}
