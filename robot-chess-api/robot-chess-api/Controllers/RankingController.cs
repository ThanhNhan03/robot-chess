using Microsoft.AspNetCore.Mvc;
using robot_chess_api.Repositories;
using robot_chess_api.DTOs;

namespace robot_chess_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RankingController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RankingController> _logger;

    public RankingController(IUserRepository userRepository, ILogger<RankingController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get global leaderboard (top players by ELO)
    /// </summary>
    [HttpGet("global")]
    public async Task<IActionResult> GetGlobalRanking([FromQuery] int limit = 100)
    {
        try
        {
            var users = await _userRepository.GetAllUsersAsync(false);
            
            var rankings = users
                .Where(u => u.IsActive)
                .OrderByDescending(u => u.EloRating)
                .ThenByDescending(u => u.Wins)
                .Take(limit)
                .Select((u, index) => new RankingDto
                {
                    Rank = index + 1,
                    UserId = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    AvatarUrl = u.AvatarUrl,
                    EloRating = u.EloRating,
                    Wins = u.Wins,
                    Losses = u.Losses,
                    Draws = u.Draws,
                    TotalGames = u.TotalGamesPlayed,
                    WinRate = u.TotalGamesPlayed > 0 
                        ? Math.Round((double)u.Wins / u.TotalGamesPlayed * 100, 1) 
                        : 0
                })
                .ToList();

            return Ok(new { success = true, rankings });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting global ranking");
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get user's ranking and nearby players
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserRanking(Guid userId, [FromQuery] int context = 5)
    {
        try
        {
            var users = await _userRepository.GetAllUsersAsync(false);
            var activeUsers = users.Where(u => u.IsActive).ToList();
            
            // Sort by ELO
            var sortedUsers = activeUsers
                .OrderByDescending(u => u.EloRating)
                .ThenByDescending(u => u.Wins)
                .ToList();

            // Find user's rank
            var userRank = sortedUsers.FindIndex(u => u.Id == userId) + 1;
            
            if (userRank == 0)
            {
                return NotFound(new { success = false, error = "User not found" });
            }

            var user = sortedUsers[userRank - 1];

            // Get nearby players
            var startIndex = Math.Max(0, userRank - 1 - context);
            var endIndex = Math.Min(sortedUsers.Count, userRank + context);
            
            var nearbyPlayers = sortedUsers
                .Skip(startIndex)
                .Take(endIndex - startIndex)
                .Select((u, index) => new RankingDto
                {
                    Rank = startIndex + index + 1,
                    UserId = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    AvatarUrl = u.AvatarUrl,
                    EloRating = u.EloRating,
                    Wins = u.Wins,
                    Losses = u.Losses,
                    Draws = u.Draws,
                    TotalGames = u.TotalGamesPlayed,
                    WinRate = u.TotalGamesPlayed > 0 
                        ? Math.Round((double)u.Wins / u.TotalGamesPlayed * 100, 1) 
                        : 0
                })
                .ToList();

            var userRanking = new RankingDto
            {
                Rank = userRank,
                UserId = user.Id,
                Username = user.Username,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                EloRating = user.EloRating,
                Wins = user.Wins,
                Losses = user.Losses,
                Draws = user.Draws,
                TotalGames = user.TotalGamesPlayed,
                WinRate = user.TotalGamesPlayed > 0 
                    ? Math.Round((double)user.Wins / user.TotalGamesPlayed * 100, 1) 
                    : 0
            };

            return Ok(new 
            { 
                success = true, 
                userRanking,
                nearbyPlayers 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user ranking");
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }

    /// <summary>
    /// Get current user's ranking
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyRanking()
    {
        try
        {
            // Extract user ID from JWT token
            var userIdClaim = User.FindFirst("sub")?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { success = false, error = "Invalid or missing token" });
            }

            return await GetUserRanking(userId, 5);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my ranking");
            return StatusCode(500, new { success = false, error = "Internal server error" });
        }
    }
}
