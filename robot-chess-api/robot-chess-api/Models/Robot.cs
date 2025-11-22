using System;
using System.Collections.Generic;

namespace robot_chess_api.Models;

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

    // New fields for robot management
    public string? IpAddress { get; set; }

    public string? TcpConnectionId { get; set; }

    public string? Status { get; set; } // idle, busy, error, maintenance

    public Guid? CurrentGameId { get; set; }

    // Navigation properties
    public virtual Game? CurrentGame { get; set; }

    public virtual ICollection<RobotCommand> RobotCommands { get; set; } = new List<RobotCommand>();

    public virtual ICollection<RobotLog> RobotLogs { get; set; } = new List<RobotLog>();

    public virtual RobotConfig? RobotConfig { get; set; }

    public virtual ICollection<RobotMonitoring> RobotMonitorings { get; set; } = new List<RobotMonitoring>();

    public virtual ICollection<RobotCommandHistory> RobotCommandHistories { get; set; } = new List<RobotCommandHistory>();
}
