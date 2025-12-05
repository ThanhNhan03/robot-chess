using System.ComponentModel.DataAnnotations;

namespace robot_chess_api.DTOs;

public class FeedbackDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserFullName { get; set; }
    public string? Message { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class CreateFeedbackDto
{
    [Required(ErrorMessage = "Message is required")]
    [MinLength(10, ErrorMessage = "Message must be at least 10 characters")]
    [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
    public string Message { get; set; } = string.Empty;
}

public class UpdateFeedbackDto
{
    [MinLength(10, ErrorMessage = "Message must be at least 10 characters")]
    [MaxLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
    public string? Message { get; set; }
}
