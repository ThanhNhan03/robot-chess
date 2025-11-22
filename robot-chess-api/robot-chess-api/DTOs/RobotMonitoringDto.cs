using System;

namespace robot_chess_api.DTOs;

/// <summary>
/// DTO cho monitoring data
/// </summary>
public class RobotMonitoringDto
{
    public Guid Id { get; set; }
    public Guid RobotId { get; set; }
    
    // Position
    public decimal? CurrentPositionX { get; set; }
    public decimal? CurrentPositionY { get; set; }
    public decimal? CurrentPositionZ { get; set; }
    
    // Rotation
    public decimal? CurrentRotationRx { get; set; }
    public decimal? CurrentRotationRy { get; set; }
    public decimal? CurrentRotationRz { get; set; }
    
    // Gripper
    public string? GripperState { get; set; }
    public int? GripperPosition { get; set; }
    
    // Status
    public bool? IsMoving { get; set; }
    public int? CurrentSpeed { get; set; }
    public bool? HasError { get; set; }
    public string? ErrorMessage { get; set; }
    
    public DateTime? RecordedAt { get; set; }
}

/// <summary>
/// DTO cho command history
/// </summary>
public class RobotCommandHistoryDto
{
    public Guid Id { get; set; }
    public Guid RobotId { get; set; }
    public string CommandType { get; set; } = null!;
    public string? Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? ExecutionTimeMs { get; set; }
    public string? ExecutedByUsername { get; set; }
}
