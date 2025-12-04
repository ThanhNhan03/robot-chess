using Microsoft.AspNetCore.Mvc;
using robot_chess_api.DTOs;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = "admin")] // Uncomment when authentication is ready
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <param name="includeInactive">Include deactivated users</param>
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers([FromQuery] bool includeInactive = false)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync(includeInactive);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting user {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create new user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error creating user");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userService.UpdateUserAsync(id, dto);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, $"User {id} not found");
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error updating user");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating user {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete user (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound($"User with ID {id} not found");
            }
            return Ok(new { Message = "User deactivated successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, $"User {id} not found");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting user {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update user active status
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusDto dto)
    {
        try
        {
            await _userService.UpdateUserStatusAsync(id, dto.IsActive);
            return Ok(new { Message = $"User {(dto.IsActive ? "activated" : "deactivated")} successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, $"User {id} not found");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating user status {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get users by role
    /// </summary>
    [HttpGet("role/{role}")]
    public async Task<ActionResult<List<UserDto>>> GetUsersByRole(string role)
    {
        try
        {
            if (!new[] { "admin", "player", "viewer" }.Contains(role.ToLower()))
            {
                return BadRequest("Role must be admin, player, or viewer");
            }

            var users = await _userService.GetUsersByRoleAsync(role.ToLower());
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting users by role {role}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user activity and statistics
    /// </summary>
    [HttpGet("{id}/activity")]
    public async Task<ActionResult<UserActivityDto>> GetUserActivity(Guid id)
    {
        try
        {
            var activity = await _userService.GetUserActivityAsync(id);
            if (activity == null)
            {
                return NotFound($"User with ID {id} not found");
            }
            return Ok(activity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting user activity {id}");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get user statistics (total users, active users, admins, new this week)
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult<UserStatsDto>> GetUserStats()
    {
        try
        {
            var stats = await _userService.GetUserStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user statistics");
            return StatusCode(500, "Internal server error");
        }
    }
}
