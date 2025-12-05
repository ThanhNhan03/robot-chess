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
    
    // Elo Rating information
    public int EloRating { get; set; }
    public int? PeakElo { get; set; }
    public int TotalGamesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
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
/// DTO for admin creating new user account
/// </summary>
public class AdminCreateUserDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "Username must not exceed 50 characters")]
    public string Username { get; set; } = null!;

    public string? FullName { get; set; }

    public string? AvatarUrl { get; set; }

    [Required(ErrorMessage = "Role is required")]
    [RegularExpression("^(admin|player|viewer)$", ErrorMessage = "Role must be admin, player, or viewer")]
    public string Role { get; set; } = "player";

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// DTO for updating user
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

/// <summary>
/// DTO for player statistics and Elo rating
/// </summary>
public class PlayerStatsDto
{
    public Guid PlayerId { get; set; }
    public string Username { get; set; } = null!;
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    
    // Elo Rating
    public int EloRating { get; set; }
    public int? PeakElo { get; set; }
    public string RatingCategory { get; set; } = null!;
    public string RatingDescription { get; set; } = null!;
    public string RatingColor { get; set; } = null!;
    
    // Game Statistics
    public int TotalGamesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public double WinRate { get; set; }
    
    // Recent performance
    public int? LastRatingChange { get; set; }
    public DateTime? LastGameAt { get; set; }
}

/// <summary>
/// DTO for leaderboard entry
/// </summary>
public class LeaderboardEntryDto
{
    public int Rank { get; set; }
    public Guid PlayerId { get; set; }
    public string Username { get; set; } = null!;
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public int EloRating { get; set; }
    public int? PeakElo { get; set; }
    public string RatingCategory { get; set; } = null!;
    public int TotalGamesPlayed { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public double WinRate { get; set; }
}

/// <summary>
/// DTO for ranking/leaderboard
/// </summary>
public class RankingDto
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string Username { get; set; } = null!;
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public int EloRating { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public int TotalGames { get; set; }
    public double WinRate { get; set; }
}

