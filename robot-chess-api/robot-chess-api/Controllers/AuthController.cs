using Microsoft.AspNetCore.Mvc;
using robot_chess_api.DTOs;
using robot_chess_api.Services.Interface;
using robot_chess_api.Repositories;
using robot_chess_api.Models;

namespace robot_chess_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IUserRepository userRepository,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userRepository = userRepository;
        _logger = logger;
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
                IsActive = true
            };

            var createdUser = await _userRepository.CreateUserAsync(appUser);

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

    // Helper method to map AppUser to UserResponse
    private static UserResponse MapToUserResponse(AppUser user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            IsActive = user.IsActive,
            LastLoginAt = user.LastLoginAt
        };
    }
}
