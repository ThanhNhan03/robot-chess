using System;
using System.ComponentModel.DataAnnotations;

namespace robot_chess_api.DTOs;

/// <summary>
/// DTO for user list view
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = "player";
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating new user
/// </summary>
public class CreateUserDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "Username must not exceed 50 characters")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = null!;

    public string? FullName { get; set; }

    public string? AvatarUrl { get; set; }

    [Required(ErrorMessage = "Role is required")]
    [RegularExpression("^(admin|player|viewer)$", ErrorMessage = "Role must be admin, player, or viewer")]
    public string Role { get; set; } = "player";

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// DTO for updating existing user
/// </summary>
public class UpdateUserDto
{
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; set; }

    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "Username must not exceed 50 characters")]
    public string? Username { get; set; }

    public string? FullName { get; set; }

    public string? AvatarUrl { get; set; }

    [RegularExpression("^(admin|player|viewer)$", ErrorMessage = "Role must be admin, player, or viewer")]
    public string? Role { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// DTO for user activity/statistics
/// </summary>
public class UserActivityDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public int TotalGames { get; set; }
    public int GamesWon { get; set; }
    public int GamesLost { get; set; }
    public int GamesDraw { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastGameAt { get; set; }
}

/// <summary>
/// DTO for updating user active status
/// </summary>
public class UpdateUserStatusDto
{
    [Required]
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for user statistics
/// </summary>
public class UserStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int AdminUsers { get; set; }
    public int NewUsersThisWeek { get; set; }
}
