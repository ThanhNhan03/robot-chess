using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_chess_api.DTOs;
using robot_chess_api.Helpers;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize(Roles = "admin")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Gửi thông báo đến players (qua email và localStorage)
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] CreateNotificationDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest(new { message = "Title is required" });
            }

            if (string.IsNullOrWhiteSpace(dto.Message))
            {
                return BadRequest(new { message = "Message is required" });
            }

            var validTypes = new[] { "info", "warning", "maintenance", "success", "error" };
            if (!validTypes.Contains(dto.Type.ToLower()))
            {
                return BadRequest(new { message = $"Type must be one of: {string.Join(", ", validTypes)}" });
            }

            var (notification, stats) = await _notificationService.SendNotificationAsync(dto);

            _logger.LogInformation(
                "Notification sent: {Title}, Recipients: {Count}, Email success: {Success}, Email failed: {Failed}",
                dto.Title, notification.RecipientCount, stats.SuccessCount, stats.FailedCount);

            return Ok(new
            {
                message = "Notification sent successfully",
                notification = notification,
                stats = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            return StatusCode(500, new { message = "Failed to send notification", error = ex.Message });
        }
    }

    /// <summary>
    /// Gửi thông báo test đến một email cụ thể (để kiểm tra)
    /// </summary>
    [HttpPost("send-test")]
    public async Task<IActionResult> SendTestNotification([FromBody] SendTestNotificationDto dto)
    {
        try
        {
            var testDto = new CreateNotificationDto
            {
                Title = dto.Title ?? "Test Notification",
                Message = dto.Message ?? "This is a test notification from Chess Robot System",
                Type = dto.Type ?? "info",
                UserIds = dto.UserId.HasValue ? new List<Guid> { dto.UserId.Value } : null,
                SendEmail = true
            };

            var (notification, stats) = await _notificationService.SendNotificationAsync(testDto);

            return Ok(new
            {
                message = "Test notification sent",
                notification = notification,
                stats = stats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test notification");
            return StatusCode(500, new { message = "Failed to send test notification", error = ex.Message });
        }
    }
}

public class SendTestNotificationDto
{
    public Guid? UserId { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? Type { get; set; }
}
