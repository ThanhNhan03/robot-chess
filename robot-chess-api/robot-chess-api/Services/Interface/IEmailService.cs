namespace robot_chess_api.Services.Interface;

public interface IEmailService
{
    /// <summary>
    /// Send email verification email to user
    /// </summary>
    Task<bool> SendVerificationEmailAsync(string toEmail, string username, string verificationToken);
    
    /// <summary>
    /// Send password reset email to user
    /// </summary>
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string username, string resetToken);
    
    /// <summary>
    /// Send welcome email to new user
    /// </summary>
    Task<bool> SendWelcomeEmailAsync(string toEmail, string username);
}
