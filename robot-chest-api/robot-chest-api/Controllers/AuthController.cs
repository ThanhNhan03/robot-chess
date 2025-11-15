using Microsoft.AspNetCore.Mvc;
using robot_chest_api.DTOs;
using robot_chest_api.Services.Interface;

namespace robot_chest_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAppUserService _appUserService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService, 
        IAppUserService appUserService, 
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _appUserService = appUserService;
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

            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password) || 
                string.IsNullOrWhiteSpace(request.Username))
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Error = "Email, password, and username are required" 
                });
            }

            // Check if username already exists
            var usernameExists = await _appUserService.UsernameExistsAsync(request.Username);
            if (usernameExists)
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
            var (profileSuccess, appUser, profileError) = await _appUserService.CreateUserProfileAsync(
                userId.Value,
                request.Email,
                request.Username
            );

            if (!profileSuccess || appUser == null)
            {
                _logger.LogError($"Failed to create profile for user: {userId}");
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Error = profileError ?? "Failed to create user profile" 
                });
            }

            _logger.LogInformation($" Sign up successful for: {request.Email}");

            return Ok(new AuthResponse
            {
                Success = true,
                Token = token,
                User = new UserResponse
                {
                    Id = appUser.Id,
                    Email = appUser.Email,
                    Username = appUser.Username,
                    FullName = appUser.FullName,
                    AvatarUrl = appUser.AvatarUrl
                }
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

            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Error = "Email and password are required" 
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
            var appUser = await _appUserService.GetUserByIdAsync(userId.Value);
            if (appUser == null)
            {
                _logger.LogError($" User profile not found for: {userId}");
                return BadRequest(new AuthResponse 
                { 
                    Success = false, 
                    Error = "User profile not found" 
                });
            }

            _logger.LogInformation($" Login successful for: {request.Email}");

            return Ok(new AuthResponse
            {
                Success = true,
                Token = token,
                User = new UserResponse
                {
                    Id = appUser.Id,
                    Email = appUser.Email,
                    Username = appUser.Username,
                    FullName = appUser.FullName,
                    AvatarUrl = appUser.AvatarUrl
                }
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
    /// Lấy thông tin user hiện tại
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            // TODO: Extract user ID from JWT token
            // For now, return placeholder
            return Ok(new { message = "Get current user endpoint - implement JWT validation" });
        }
        catch (Exception ex)
        {
            _logger.LogError($" GetCurrentUser exception: {ex.Message}");
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }
}
