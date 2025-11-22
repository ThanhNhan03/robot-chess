using System;

namespace robot_chess_api.DTOs;

/// <summary>
/// DTO cho thông tin robot đầy đủ
/// </summary>
public class RobotDto
{
    public Guid Id { get; set; }
    public string RobotCode { get; set; } = null!;
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? IpAddress { get; set; }
    public bool? IsOnline { get; set; }
    public DateTime? LastOnlineAt { get; set; }
    public string? Status { get; set; }
    public Guid? CurrentGameId { get; set; }
    public int? MoveSpeedMs { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Config info
    public RobotConfigDto? Config { get; set; }

    // Latest monitoring info
    public RobotMonitoringDto? LatestMonitoring { get; set; }
}

/// <summary>
/// DTO cho tạo robot mới
/// </summary>
public class CreateRobotDto
{
    public string RobotCode { get; set; } = null!;
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? IpAddress { get; set; }
}

/// <summary>
/// DTO cho cập nhật robot
/// </summary>
public class UpdateRobotDto
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? IpAddress { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// DTO cho robot status realtime
/// </summary>
public class RobotStatusDto
{
    public Guid Id { get; set; }
    public string RobotCode { get; set; } = null!;
    public string? Name { get; set; }
    public bool? IsOnline { get; set; }
    public DateTime? LastOnlineAt { get; set; }
    public string? Status { get; set; }
    public bool? IsMoving { get; set; }
    public string? GripperState { get; set; }
    public bool? HasError { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? LastMonitoringAt { get; set; }
}
