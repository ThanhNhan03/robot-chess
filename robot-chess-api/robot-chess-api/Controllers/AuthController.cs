using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using robot_chess_api.DTOs;
using robot_chess_api.Services.Interface;
using robot_chess_api.Repositories;
using robot_chess_api.Models;
using robot_chess_api.Hubs;
using System.Security.Cryptography;

namespace robot_chess_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;
    private readonly IHubContext<AuthHub> _hubContext;

    public AuthController(
        IAuthService authService,
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<AuthController> logger,
        IHubContext<AuthHub> hubContext)
    {
        _authService = authService;
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Đăng ký tài khoản mới
    /// </summary>
    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
    {
        try
        {
            _logger.LogInformation($"Sign up request for: {request.Email}");

            // Validate model
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Error = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage))
                });
            }

            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(request.Email))
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Error = "Email already exists"
                });
            }

            // Check if username already exists
            if (await _userRepository.UsernameExistsAsync(request.Username))
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Error = "Username already exists"
                });
            }

            // 1. Supabase Auth sign-up
            var (authSuccess, token, userId, authError) = await _authService.SignUpAsync(
                request.Email,
                request.Password
            );

            if (!authSuccess || userId == null)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Error = authError ?? "Sign up failed"
                });
            }

            // 2. Create AppUser profile
            var appUser = new AppUser
            {
                Id = userId.Value,
                Email = request.Email,
                Username = request.Username,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Role = "player", // Default role
                IsActive = true,
                EmailVerified = false,
                EmailVerificationToken = GenerateVerificationToken(),
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24)
            };

            var createdUser = await _userRepository.CreateUserAsync(appUser);

            // 3. Send verification email
            try
            {
                await _emailService.SendVerificationEmailAsync(
                    createdUser.Email,
                    createdUser.Username,
                    createdUser.EmailVerificationToken!
                );
                _logger.LogInformation($"Verification email sent to: {request.Email}");
            }
            catch (Exception emailEx)
            {
                _logger.LogError($"Failed to send verification email: {emailEx.Message}");
                // Continue anyway - user is created
            }

            _logger.LogInformation($"Sign up successful for: {request.Email}");

            return Ok(new AuthResponse
            {
                Success = true,
                Token = token,
                User = MapToUserResponse(createdUser)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Sign up exception: {ex.Message}");
            return StatusCode(500, new AuthResponse
            {
                Success = false,
                Error = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Đăng nhập
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation($"Login request for: {request.Email}");

            // Validate model
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Error = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage))
                });
            }

            // 1. Supabase Auth login
            var (success, token, userId, error) = await _authService.LoginAsync(
                request.Email,
                request.Password
            );

            if (!success || userId == null)
            {
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Error = error ?? "Invalid email or password"
                });
            }

            // 2. Get AppUser profile
            var appUser = await _userRepository.GetUserByIdAsync(userId.Value);
            if (appUser == null)
            {
                _logger.LogError($"User profile not found for: {userId}");
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Error = "User profile not found"
                });
            }

            // Check if user is active
            if (!appUser.IsActive)
            {
                return Unauthorized(new AuthResponse
                {
                    Success = false,
                    Error = "Account has been deactivated"
                });
            }

            // 3. Update last login time
            await _userRepository.UpdateLastLoginAsync(userId.Value);
            appUser.LastLoginAt = DateTime.UtcNow; // Update local object for response

            _logger.LogInformation($"Login successful for: {request.Email}");

            return Ok(new AuthResponse
            {
                Success = true,
                Token = token,
                User = MapToUserResponse(appUser)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login exception: {ex.Message}");
            return StatusCode(500, new AuthResponse
            {
                Success = false,
                Error = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Đăng xuất
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var (success, error) = await _authService.LogoutAsync(token);

            if (!success)
            {
                return BadRequest(new { success = false, error });
            }

            return Ok(new { success = true, message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Logout exception: {ex.Message}");
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }

    /// <summary>
    /// Lấy thông tin user hiện tại từ token
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            // Extract user ID from JWT token (implement JWT middleware first)
            var userIdClaim = User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { success = false, error = "Invalid or missing token" });
            }

            var appUser = await _userRepository.GetUserByIdAsync(userId);
            if (appUser == null)
            {
                return NotFound(new { success = false, error = "User not found" });
            }

            return Ok(new
            {
                success = true,
                user = MapToUserResponse(appUser)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"GetCurrentUser exception: {ex.Message}");
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }

    /// <summary>
    /// Cập nhật profile user
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            // Extract user ID from JWT token
            var userIdClaim = User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { success = false, error = "Invalid or missing token" });
            }

            var appUser = await _userRepository.GetUserByIdAsync(userId);
            if (appUser == null)
            {
                return NotFound(new { success = false, error = "User not found" });
            }

            // Check if username is being changed and if it already exists
            if (request.Username != null && request.Username != appUser.Username)
            {
                if (await _userRepository.UsernameExistsAsync(request.Username))
                {
                    return BadRequest(new { success = false, error = "Username already exists" });
                }
                appUser.Username = request.Username;
            }

            // Update fields
            if (request.FullName != null) appUser.FullName = request.FullName;
            if (request.AvatarUrl != null) appUser.AvatarUrl = request.AvatarUrl;
            if (request.PhoneNumber != null) appUser.PhoneNumber = request.PhoneNumber;

            var updatedUser = await _userRepository.UpdateUserAsync(appUser);

            return Ok(new
            {
                success = true,
                user = MapToUserResponse(updatedUser)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"UpdateProfile exception: {ex.Message}");
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }

    /// <summary>
    /// Đổi mật khẩu
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            // Extract user ID from JWT token
            var userIdClaim = User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { success = false, error = "Invalid or missing token" });
            }

            var appUser = await _userRepository.GetUserByIdAsync(userId);
            if (appUser == null)
            {
                return NotFound(new { success = false, error = "User not found" });
            }

            // Verify current password with Supabase
            var (loginSuccess, _, _, loginError) = await _authService.SignInAsync(
                appUser.Email, 
                request.CurrentPassword
            );

            if (!loginSuccess)
            {
                return BadRequest(new { success = false, error = "Current password is incorrect" });
            }

            // Update password in Supabase
            var (updateSuccess, updateError) = await _authService.UpdatePasswordAsync(
                userId,
                request.NewPassword
            );

            if (!updateSuccess)
            {
                return BadRequest(new { success = false, error = updateError ?? "Failed to update password" });
            }

            _logger.LogInformation($"Password changed successfully for user: {appUser.Email}");

            return Ok(new
            {
                success = true,
                message = "Password changed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"ChangePassword exception: {ex.Message}");
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }

    /// <summary>
    /// Xác thực email với token
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        try
        {
            _logger.LogInformation($"Email verification request with token");

            if (string.IsNullOrEmpty(request.Token))
            {
                return BadRequest(new { success = false, error = "Verification token is required" });
            }

            // Find user by verification token
            var user = await _userRepository.GetUserByVerificationTokenAsync(request.Token);

            if (user == null)
            {
                return BadRequest(new { success = false, error = "Invalid verification token" });
            }

            // Check if token has expired
            if (user.EmailVerificationTokenExpiry == null || 
                user.EmailVerificationTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { success = false, error = "Verification token has expired" });
            }

            // Check if already verified
            if (user.EmailVerified)
            {
                return Ok(new { success = true, message = "Email already verified" });
            }

            // Update user verification status
            await _userRepository.UpdateEmailVerificationAsync(user.Id, true);

            _logger.LogInformation($"Email verified successfully for user: {user.Email}");

            return Ok(new 
            { 
                success = true, 
                message = "Email verified successfully. You can now login." 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"VerifyEmail exception: {ex.Message}");
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gửi lại email xác thực
    /// </summary>
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerificationEmail([FromBody] ResendVerificationRequest request)
    {
        try
        {
            _logger.LogInformation($"Resend verification email request for: {request.Email}");

            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                // Don't reveal if email exists or not
                return Ok(new { success = true, message = "If the email exists, a verification email has been sent" });
            }

            if (user.EmailVerified)
            {
                return BadRequest(new { success = false, error = "Email already verified" });
            }

            // Generate new verification token
            user.EmailVerificationToken = GenerateVerificationToken();
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
            await _userRepository.UpdateUserAsync(user);

            // Send verification email
            await _emailService.SendVerificationEmailAsync(
                user.Email,
                user.Username,
                user.EmailVerificationToken
            );

            _logger.LogInformation($"Verification email resent to: {request.Email}");

            return Ok(new 
            { 
                success = true, 
                message = "Verification email sent. Please check your inbox." 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"ResendVerificationEmail exception: {ex.Message}");
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }

    /// <summary>
    /// Yêu cầu đặt lại mật khẩu
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            _logger.LogInformation($"Forgot password request for: {request.Email}");

            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user == null)
            {
                // Don't reveal if email exists or not
                return Ok(new { success = true, message = "If the email exists, a password reset link has been sent" });
            }

            // Generate password reset token
            user.PasswordResetToken = GenerateVerificationToken();
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // 1 hour expiry
            await _userRepository.UpdateUserAsync(user);

            // Send password reset email
            await _emailService.SendPasswordResetEmailAsync(
                user.Email,
                user.Username,
                user.PasswordResetToken
            );

            _logger.LogInformation($"Password reset email sent to: {request.Email}");

            return Ok(new 
            { 
                success = true, 
                message = "Password reset email sent. Please check your inbox." 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"ForgotPassword exception: {ex.Message}");
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }

    /// <summary>
    /// Đặt lại mật khẩu với token
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            _logger.LogInformation($"Reset password request with token");

            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new { success = false, error = "Token is required" });
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { success = false, error = "New password is required" });
            }

            // Find user by reset token
            var user = await _userRepository.GetUserByPasswordResetTokenAsync(request.Token);

            if (user == null)
            {
                return BadRequest(new { success = false, error = "Invalid or expired reset token" });
            }

            // Check if token is expired
            if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
            {
                return BadRequest(new { success = false, error = "Reset token has expired" });
            }

            // Update password in Supabase Auth
            var (success, error) = await _authService.UpdatePasswordAsync(user.Id, request.NewPassword);

            if (!success)
            {
                return BadRequest(new { success = false, error = error ?? "Failed to update password" });
            }

            // Clear reset token
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;
            await _userRepository.UpdateUserAsync(user);

            _logger.LogInformation($"Password reset successful for user: {user.Email}");

            return Ok(new 
            { 
                success = true, 
                message = "Password has been reset successfully" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"ResetPassword exception: {ex.Message}");
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }

    // Helper method to generate verification token
    private static string GenerateVerificationToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    // Helper method to map AppUser to UserResponse
    private static UserResponse MapToUserResponse(AppUser user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt,
            PointsBalance = user.PointsBalance,
            EloRating = user.EloRating,
            PeakElo = user.PeakElo,
            TotalGamesPlayed = user.TotalGamesPlayed,
            Wins = user.Wins,
            Losses = user.Losses,
            Draws = user.Draws
        };
    }

    /// <summary>
    /// Generate QR code session for login
    /// </summary>
    [HttpPost("qr-session")]
    public IActionResult GenerateQRSession()
    {
        try
        {
            var sessionId = Guid.NewGuid().ToString();
            var expiryTime = DateTime.UtcNow.AddMinutes(5);

            _logger.LogInformation($"QR session generated: {sessionId}");

            // Use web URL instead of deeplink - can be scanned by any device
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var qrUrl = $"{baseUrl}/auth/qr-callback?session={sessionId}";

            return Ok(new
            {
                sessionId,
                expiryTime,
                qrData = qrUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"QR session generation error: {ex.Message}");
            return StatusCode(500, new { error = "Failed to generate QR session" });
        }
    }

    /// <summary>
    /// QR callback page calls this endpoint after OAuth success to complete QR login
    /// </summary>
    [HttpPost("qr-login")]
    public async Task<IActionResult> QRLogin([FromBody] QRLoginRequest request)
    {
        try
        {
            _logger.LogInformation($"QR login request for session: {request.SessionId}");

            // Validate model
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Error = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage))
                });
            }

            // Verify Google token with Supabase
            var (success, userId, email, authError) = await _authService.VerifyGoogleTokenAsync(request.AccessToken);

            if (!success || userId == null)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Error = authError ?? "Google authentication failed"
                });
            }

            // Get or create user
            var user = await _userRepository.GetUserByIdAsync(userId.Value);

            if (user == null)
            {
                // Create new user profile
                user = new AppUser
                {
                    Id = userId.Value,
                    Email = email!,
                    Username = request.Username ?? email!.Split('@')[0],
                    FullName = request.FullName,
                    AvatarUrl = request.AvatarUrl,
                    Role = "player",
                    IsActive = true,
                    EmailVerified = true, // Google accounts are pre-verified
                    CreatedAt = DateTime.UtcNow,
                    EloRating = 1200,
                    PointsBalance = 0
                };

                user = await _userRepository.CreateUserAsync(user);
                _logger.LogInformation($"New Google user created via QR: {email}");
            }
            else
            {
                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                _logger.LogInformation($"Existing user logged in via QR: {email}");
            }

            // Generate JWT token
            var token = await _authService.GenerateJwtTokenAsync(user);

            // Notify web client via SignalR
            await _hubContext.Clients.All.SendAsync(
                "NotifyLoginSuccess",
                request.SessionId,
                token,
                MapToUserResponse(user)
            );

            _logger.LogInformation($"Login success notification sent for session: {request.SessionId}");

            return Ok(new AuthResponse
            {
                Success = true,
                Token = token,
                User = MapToUserResponse(user)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"QR login exception: {ex.Message}");
            return StatusCode(500, new AuthResponse
            {
                Success = false,
                Error = "Internal server error during QR login"
            });
        }
    }

    /// <summary>
    /// Direct Google sign-in (not QR-based, for mobile/web direct OAuth)
    /// </summary>
    [HttpPost("google-signin")]
    public async Task<IActionResult> GoogleSignIn([FromBody] GoogleSignInRequest request)
    {
        try
        {
            _logger.LogInformation("Google sign-in request received");

            // Validate model
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Error = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage))
                });
            }

            // Verify token with Supabase
            var (success, userId, email, authError) = await _authService.VerifyGoogleTokenAsync(request.AccessToken);

            if (!success || userId == null)
            {
                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Error = authError ?? "Google authentication failed"
                });
            }

            // Check if user exists
            var existingUser = await _userRepository.GetUserByIdAsync(userId.Value);

            if (existingUser == null)
            {
                // Create new user profile
                var newUser = new AppUser
                {
                    Id = userId.Value,
                    Email = email!,
                    Username = request.Username ?? email!.Split('@')[0],
                    FullName = request.FullName,
                    AvatarUrl = request.AvatarUrl,
                    Role = "player",
                    IsActive = true,
                    EmailVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    EloRating = 1200,
                    PointsBalance = 0
                };

                existingUser = await _userRepository.CreateUserAsync(newUser);
                _logger.LogInformation($"New Google user created: {email}");
            }
            else
            {
                // Update last login
                existingUser.LastLoginAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(existingUser);
            }

            // Generate JWT token
            var token = await _authService.GenerateJwtTokenAsync(existingUser);

            return Ok(new AuthResponse
            {
                Success = true,
                Token = token,
                User = MapToUserResponse(existingUser)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Google sign-in exception: {ex.Message}");
            return StatusCode(500, new AuthResponse
            {
                Success = false,
                Error = "Internal server error"
            });
        }
    }
}

