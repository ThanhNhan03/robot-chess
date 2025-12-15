namespace robot_chess_api.DTOs;

public class CreateNotificationDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "info"; // info, warning, maintenance, success, error
    public List<Guid>? UserIds { get; set; } // null = gửi cho tất cả players
    public List<string>? UserEmails { get; set; } // null = gửi cho tất cả players, có thể dùng emails thay vì IDs
    public bool SendEmail { get; set; } = true;
}

public class NotificationResponseDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int RecipientCount { get; set; }
    public bool EmailSent { get; set; }
}

public class NotificationStatsDto
{
    public int TotalEmailsSent { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> FailedEmails { get; set; } = new();
}
