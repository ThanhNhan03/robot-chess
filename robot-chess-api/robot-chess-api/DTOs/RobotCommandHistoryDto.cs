using System;
using System.Text.Json;

namespace robot_chess_api.DTOs;

/// <summary>
/// DTO for robot command history
/// </summary>
public class RobotCommandHistoryDto
{
    public Guid Id { get; set; }
    public Guid RobotId { get; set; }
    public string CommandType { get; set; } = null!;
    public JsonDocument? Payload { get; set; }
    public string Status { get; set; } = null!;
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? ExecutionTimeMs { get; set; }
    public Guid? ExecutedBy { get; set; }
}
