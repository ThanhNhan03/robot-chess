using System;
using System.Collections.Generic;

namespace robot_chest_api.Data;

public partial class Robot
{
    public Guid Id { get; set; }

    public string RobotCode { get; set; } = null!;

    public string? Name { get; set; }

    public string? Location { get; set; }

    public bool? IsOnline { get; set; }

    public DateTime? LastOnlineAt { get; set; }

    public int? MoveSpeedMs { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<RobotCommand> RobotCommands { get; set; } = new List<RobotCommand>();

    public virtual ICollection<RobotLog> RobotLogs { get; set; } = new List<RobotLog>();
}
