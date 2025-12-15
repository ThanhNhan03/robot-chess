using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using robot_chess_api.DTOs;
using robot_chess_api.Repositories;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Services.Implement;

public class NotificationService : INotificationService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<NotificationService> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(NotificationResponseDto notification, NotificationStatsDto stats)> SendNotificationAsync(CreateNotificationDto dto)
    {
        // Lấy danh sách users cần gửi
        var users = dto.UserIds != null && dto.UserIds.Any()
            ? await _userRepository.GetUsersByIdsAsync(dto.UserIds)
            : await _userRepository.GetAllPlayersAsync();

        var notification = new NotificationResponseDto
        {
            Id = Guid.NewGuid().ToString(),
            Title = dto.Title,
            Message = dto.Message,
            Type = dto.Type,
            CreatedAt = DateTime.UtcNow,
            RecipientCount = users.Count
        };

        var stats = new NotificationStatsDto
        {
            TotalEmailsSent = users.Count
        };

        // Gửi email nếu được yêu cầu
        if (dto.SendEmail)
        {
            foreach (var user in users)
            {
                if (!string.IsNullOrEmpty(user.Email))
                {
                    try
                    {
                        await SendEmailAsync(user.Email, user.FullName ?? user.Username, dto.Title, dto.Message, dto.Type);
                        stats.SuccessCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email to {Email}", user.Email);
                        stats.FailedCount++;
                        stats.FailedEmails.Add(user.Email);
                    }
                }
            }
            notification.EmailSent = true;
        }

        return (notification, stats);
    }

    private async Task SendEmailAsync(string toEmail, string userName, string title, string message, string type)
    {
        var smtpHost = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        var fromEmail = _configuration["EmailSettings:SenderEmail"] ?? "";
        var fromPassword = _configuration["EmailSettings:Password"] ?? "";
        var fromName = _configuration["EmailSettings:SenderName"] ?? "Robot Chess";

        if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromPassword))
        {
            throw new InvalidOperationException("Email configuration is missing");
        }

        var mailMessage = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = $"[Chess Robot] {title}",
            Body = GenerateEmailBody(userName, title, message, type),
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        using var smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(fromEmail, fromPassword),
            EnableSsl = true
        };

        await smtpClient.SendMailAsync(mailMessage);
    }

    private string GenerateEmailBody(string userName, string title, string message, string type)
    {
        var typeColor = type.ToLower() switch
        {
            "warning" => "#ff9800",
            "error" => "#f44336",
            "success" => "#4caf50",
            "maintenance" => "#2196f3",
            _ => "#607d8b"
        };

        var typeIcon = type.ToLower() switch
        {
            "warning" => "⚠️",
            "error" => "❌",
            "success" => "✅",
            "maintenance" => "🔧",
            _ => "ℹ️"
        };

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .notification {{ background: white; padding: 20px; border-left: 4px solid {typeColor}; border-radius: 5px; margin: 20px 0; }}
        .notification-title {{ font-size: 18px; font-weight: bold; color: {typeColor}; margin-bottom: 10px; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>♟️ Chess Robot System</h1>
        </div>
        <div class='content'>
            <p>Xin chào <strong>{userName}</strong>,</p>
            
            <div class='notification'>
                <div class='notification-title'>{typeIcon} {title}</div>
                <p>{message.Replace("\n", "<br>")}</p>
            </div>

            <p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>
            
            <center>
                <a href='{_configuration["AppUrl"] ?? "http://localhost:5173"}' class='button'>Truy cập hệ thống</a>
            </center>
        </div>
        <div class='footer'>
            <p>© 2025 Chess Robot System. All rights reserved.</p>
            <p>Email này được gửi tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
    }
}
