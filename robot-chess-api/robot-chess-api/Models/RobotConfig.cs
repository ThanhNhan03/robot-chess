using System;

namespace robot_chess_api.Models;

/// <summary>
/// Cấu hình tốc độ và thông số điều khiển robot
/// </summary>
public partial class RobotConfig
{
    public Guid Id { get; set; }

    public Guid RobotId { get; set; }

    /// <summary>
    /// Tốc độ robot (10-100), tương ứng với robot.SetSpeed() trong Python
    /// </summary>
    public int? Speed { get; set; }

    /// <summary>
    /// Lực kẹp của gripper (0-100)
    /// </summary>
    public int? GripperForce { get; set; }

    /// <summary>
    /// Tốc độ gripper (0-100)
    /// </summary>
    public int? GripperSpeed { get; set; }

    /// <summary>
    /// Tốc độ tối đa cho phép
    /// </summary>
    public int? MaxSpeed { get; set; }

    /// <summary>
    /// Cờ dừng khẩn cấp
    /// </summary>
    public bool? EmergencyStop { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    // Navigation properties
    public virtual Robot Robot { get; set; } = null!;

    public virtual AppUser? UpdatedByUser { get; set; }
}
