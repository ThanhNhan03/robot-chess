using System.Net;
using System.Net.Mail;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Services.Implement;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _senderEmail;
    private readonly string _senderName;
    private readonly string _username;
    private readonly string _password;
    private readonly bool _enableSsl;
    private readonly string _frontendUrl;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        _smtpServer = _configuration["EmailSettings:SmtpServer"] ?? throw new ArgumentNullException("EmailSettings:SmtpServer");
        _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? throw new ArgumentNullException("EmailSettings:SenderEmail");
        _senderName = _configuration["EmailSettings:SenderName"] ?? "Robot Chess";
        _username = _configuration["EmailSettings:Username"] ?? throw new ArgumentNullException("EmailSettings:Username");
        _password = _configuration["EmailSettings:Password"] ?? throw new ArgumentNullException("EmailSettings:Password");
        _enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");
        _frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:5173";
    }

    public async Task<bool> SendVerificationEmailAsync(string toEmail, string username, string verificationToken)
    {
        try
        {
            var verificationLink = $"{_frontendUrl}/verify-email?token={verificationToken}";
            
            var subject = "Xác thực tài khoản Robot Chess";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .button {{ 
                            display: inline-block; 
                            padding: 12px 24px; 
                            background-color: #4CAF50; 
                            color: white; 
                            text-decoration: none; 
                            border-radius: 4px;
                            margin: 20px 0;
                        }}
                        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🤖 Robot Chess</h1>
                        </div>
                        <div class='content'>
                            <h2>Xin chào {username}!</h2>
                            <p>Cảm ơn bạn đã đăng ký tài khoản tại Robot Chess.</p>
                            <p>Vui lòng nhấp vào nút bên dưới để xác thực email của bạn:</p>
                            <p style='text-align: center;'>
                                <a href='{verificationLink}' class='button'>Xác thực Email</a>
                            </p>
                            <p>Hoặc copy link sau vào trình duyệt:</p>
                            <p style='word-break: break-all;'>{verificationLink}</p>
                            <p><strong>Lưu ý:</strong> Link xác thực sẽ hết hạn sau 24 giờ.</p>
                            <p>Nếu bạn không đăng ký tài khoản này, vui lòng bỏ qua email này.</p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Robot Chess. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            return await SendEmailAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending verification email to {toEmail}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string username, string resetToken)
    {
        try
        {
            var resetLink = $"{_frontendUrl}/reset-password?token={resetToken}";
            
            var subject = "Đặt lại mật khẩu Robot Chess";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #FF5722; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .button {{ 
                            display: inline-block; 
                            padding: 12px 24px; 
                            background-color: #FF5722; 
                            color: white; 
                            text-decoration: none; 
                            border-radius: 4px;
                            margin: 20px 0;
                        }}
                        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🤖 Robot Chess</h1>
                        </div>
                        <div class='content'>
                            <h2>Xin chào {username}!</h2>
                            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                            <p>Vui lòng nhấp vào nút bên dưới để đặt lại mật khẩu:</p>
                            <p style='text-align: center;'>
                                <a href='{resetLink}' class='button'>Đặt lại mật khẩu</a>
                            </p>
                            <p>Hoặc copy link sau vào trình duyệt:</p>
                            <p style='word-break: break-all;'>{resetLink}</p>
                            <p><strong>Lưu ý:</strong> Link đặt lại mật khẩu sẽ hết hạn sau 1 giờ.</p>
                            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Robot Chess. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            return await SendEmailAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending password reset email to {toEmail}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string username)
    {
        try
        {
            var subject = "Chào mừng đến với Robot Chess!";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .button {{ 
                            display: inline-block; 
                            padding: 12px 24px; 
                            background-color: #2196F3; 
                            color: white; 
                            text-decoration: none; 
                            border-radius: 4px;
                            margin: 20px 0;
                        }}
                        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🤖 Robot Chess</h1>
                        </div>
                        <div class='content'>
                            <h2>Chào mừng {username}!</h2>
                            <p>Tài khoản của bạn đã được tạo thành công!</p>
                            <p>Bạn có thể bắt đầu trải nghiệm chơi cờ vua với robot AI của chúng tôi.</p>
                            <p style='text-align: center;'>
                                <a href='{_frontendUrl}/login' class='button'>Đăng nhập ngay</a>
                            </p>
                            <h3>Tính năng nổi bật:</h3>
                            <ul>
                                <li>Chơi cờ vua với Robot AI</li>
                                <li>Theo dõi lịch sử ván đấu</li>
                                <li>Xem bảng xếp hạng</li>
                                <li>Luyện tập với các bài toán cờ</li>
                            </ul>
                            <p>Chúc bạn có những trải nghiệm tuyệt vời!</p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Robot Chess. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            return await SendEmailAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending welcome email to {toEmail}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendAccountCreatedEmailAsync(string toEmail, string username, string password)
    {
        try
        {
            var subject = "Tài khoản Robot Chess của bạn đã được tạo";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #673AB7; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .credentials {{ 
                            background-color: #fff; 
                            padding: 15px; 
                            border-left: 4px solid #673AB7;
                            margin: 20px 0;
                        }}
                        .button {{ 
                            display: inline-block; 
                            padding: 12px 24px; 
                            background-color: #673AB7; 
                            color: white; 
                            text-decoration: none; 
                            border-radius: 4px;
                            margin: 20px 0;
                        }}
                        .warning {{ 
                            background-color: #fff3cd; 
                            padding: 10px; 
                            border-left: 4px solid #ffc107;
                            margin: 20px 0;
                        }}
                        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>🤖 Robot Chess</h1>
                        </div>
                        <div class='content'>
                            <h2>Xin chào {username}!</h2>
                            <p>Tài khoản Robot Chess của bạn đã được quản trị viên tạo thành công.</p>
                            
                            <div class='credentials'>
                                <h3>Thông tin đăng nhập:</h3>
                                <p><strong>Email:</strong> {toEmail}</p>
                                <p><strong>Mật khẩu:</strong> {password}</p>
                            </div>
                            
                            <div class='warning'>
                                <p><strong>⚠️ Lưu ý bảo mật:</strong></p>
                                <ul style='margin: 5px 0;'>
                                    <li>Vui lòng đổi mật khẩu ngay sau khi đăng nhập lần đầu</li>
                                    <li>Không chia sẻ thông tin đăng nhập với người khác</li>
                                    <li>Sử dụng mật khẩu mạnh kết hợp chữ, số và ký tự đặc biệt</li>
                                </ul>
                            </div>
                            
                            <p style='text-align: center;'>
                                <a href='{_frontendUrl}/login' class='button'>Đăng nhập ngay</a>
                            </p>
                            
                            <h3>Bắt đầu với Robot Chess:</h3>
                            <ul>
                                <li>🎮 Chơi cờ vua với Robot AI thông minh</li>
                                <li>📊 Theo dõi lịch sử ván đấu và thống kê</li>
                                <li>🏆 Xem bảng xếp hạng ELO</li>
                                <li>🧩 Luyện tập với các bài toán cờ</li>
                            </ul>
                            
                            <p>Nếu bạn cần hỗ trợ, vui lòng liên hệ với quản trị viên.</p>
                        </div>
                        <div class='footer'>
                            <p>© 2024 Robot Chess. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            return await SendEmailAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending account created email to {toEmail}: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_senderEmail, _senderName);
            message.To.Add(new MailAddress(toEmail));
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using var smtpClient = new SmtpClient(_smtpServer, _smtpPort);
            smtpClient.Credentials = new NetworkCredential(_username, _password);
            smtpClient.EnableSsl = _enableSsl;

            await smtpClient.SendMailAsync(message);
            _logger.LogInformation($"Email sent successfully to {toEmail}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending email to {toEmail}: {ex.Message}");
            throw;
        }
    }
}
