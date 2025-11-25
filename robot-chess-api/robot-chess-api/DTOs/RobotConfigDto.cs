using System;

namespace robot_chess_api.DTOs;

/// <summary>
/// DTO cho cấu hình robot
/// </summary>
public class RobotConfigDto
{
    public Guid Id { get; set; }
    public Guid RobotId { get; set; }
    public int? Speed { get; set; }
    public int? GripperForce { get; set; }
    public int? GripperSpeed { get; set; }
    public int? MaxSpeed { get; set; }
    public bool? EmergencyStop { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO cho cập nhật config robot
/// </summary>
public class UpdateRobotConfigDto
{
    public int? Speed { get; set; }
    public int? GripperForce { get; set; }
    public int? GripperSpeed { get; set; }
    public int? MaxSpeed { get; set; }
}
