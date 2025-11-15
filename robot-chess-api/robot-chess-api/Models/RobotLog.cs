using System;
using System.Collections.Generic;

namespace robot_chess_api.Models;

public partial class RobotLog
{
    public Guid Id { get; set; }

    public Guid? CommandId { get; set; }

    public string? LogMessage { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid? RobotId { get; set; }

    public virtual RobotCommand? Command { get; set; }

    public virtual Robot? Robot { get; set; }
}
