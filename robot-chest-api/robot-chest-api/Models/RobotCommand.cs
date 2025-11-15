using System;
using System.Collections.Generic;

namespace robot_chest_api.Models;

public partial class RobotCommand
{
    public Guid Id { get; set; }

    public Guid? GameId { get; set; }

    public string Command { get; set; } = null!;

    public string? Status { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime? ExecutedAt { get; set; }

    public Guid? RobotId { get; set; }

    public virtual Robot? Robot { get; set; }

    public virtual ICollection<RobotLog> RobotLogs { get; set; } = new List<RobotLog>();
}
