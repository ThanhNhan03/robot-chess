using System;
using System.Text.Json;

namespace robot_chess_api.Models;

/// <summary>
/// Lịch sử các lệnh đã gửi đến robot
/// </summary>
public partial class RobotCommandHistory
{
    public Guid Id { get; set; }

    public Guid RobotId { get; set; }

    /// <summary>
    /// Loại lệnh: move_piece, home, calibrate, test_gripper, emergency_stop, set_speed
    /// </summary>
    public string CommandType { get; set; } = null!;

    /// <summary>
    /// Chi tiết lệnh (JSON)
    /// </summary>
    public JsonDocument? CommandPayload { get; set; }

    /// <summary>
    /// Trạng thái: pending, executing, completed, failed, cancelled
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Kết quả thực thi (JSON)
    /// </summary>
    public JsonDocument? ResultPayload { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Thời gian thực thi (milliseconds)
    /// </summary>
    public int? ExecutionTimeMs { get; set; }

    /// <summary>
    /// Người thực hiện (nếu từ admin)
    /// </summary>
    public Guid? ExecutedBy { get; set; }

    // Navigation properties
    public virtual Robot Robot { get; set; } = null!;

    public virtual AppUser? ExecutedByUser { get; set; }
}
