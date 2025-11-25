using System;

namespace robot_chess_api.Models;

/// <summary>
/// Lưu trữ thông tin monitoring realtime của robot
/// </summary>
public partial class RobotMonitoring
{
    public Guid Id { get; set; }

    public Guid RobotId { get; set; }

    // Thông tin vị trí hiện tại
    public decimal? CurrentPositionX { get; set; }

    public decimal? CurrentPositionY { get; set; }

    public decimal? CurrentPositionZ { get; set; }

    // Thông tin góc quay
    public decimal? CurrentRotationRx { get; set; }

    public decimal? CurrentRotationRy { get; set; }

    public decimal? CurrentRotationRz { get; set; }

    // Trạng thái gripper
    public string? GripperState { get; set; } // open, closed, moving

    public int? GripperPosition { get; set; }

    // Thông tin hoạt động
    public bool? IsMoving { get; set; }

    public int? CurrentSpeed { get; set; }

    public Guid? CurrentCommandId { get; set; }

    // Thông tin lỗi
    public bool? HasError { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? RecordedAt { get; set; }

    // Navigation properties
    public virtual Robot Robot { get; set; } = null!;

    public virtual RobotCommand? CurrentCommand { get; set; }
}
