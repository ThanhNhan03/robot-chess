using System.ComponentModel.DataAnnotations;

namespace robot_chess_api.DTOs;

/// <summary>
/// DTO cho cập nhật tốc độ robot
/// </summary>
public class UpdateSpeedDto
{
    /// <summary>
    /// Tốc độ robot (10-100)
    /// </summary>
    [Required]
    [Range(10, 100, ErrorMessage = "Speed must be between 10 and 100")]
    public int Speed { get; set; }
}

/// <summary>
/// DTO cho lệnh home
/// </summary>
public class HomeCommandDto
{
    // Không cần parameters, chỉ là trigger
}

/// <summary>
/// DTO cho lệnh test gripper
/// </summary>
public class TestGripperDto
{
    [Range(0, 100)]
    public int? Position { get; set; }
}

/// <summary>
/// DTO cho lệnh calibrate
/// </summary>
public class CalibrateDto
{
    public string? CalibrationType { get; set; } // "corners", "full", "gripper"
}

/// <summary>
/// DTO cho emergency stop
/// </summary>
public class EmergencyStopDto
{
    // Không cần parameters
}
