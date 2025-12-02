using System;

namespace robot_chess_api.Helpers;

/// <summary>
/// Helper class for calculating and managing Elo ratings based on FIDE standard
/// </summary>
public class EloRatingHelper
{
    // K-factor constants (determines how much rating changes per game)
    // Higher K-factor means bigger rating swings
    private const int K_FACTOR_BEGINNER = 32;      // For players with rating < 2000
    private const int K_FACTOR_INTERMEDIATE = 24;  // For players with rating 2000-2400
    private const int K_FACTOR_MASTER = 16;        // For players with rating >= 2400

    /// <summary>
    /// Calculate K-factor based on player's current rating
    /// </summary>
    public static int GetKFactor(int currentRating)
    {
        if (currentRating < 2000)
            return K_FACTOR_BEGINNER;
        if (currentRating < 2400)
            return K_FACTOR_INTERMEDIATE;
        return K_FACTOR_MASTER;
    }

    /// <summary>
    /// Calculate expected score (probability of winning) against opponent
    /// Using FIDE formula: E = 1 / (1 + 10^((Opponent Rating - Your Rating) / 400))
    /// </summary>
    public static double CalculateExpectedScore(int playerRating, int opponentRating)
    {
        double ratingDifference = opponentRating - playerRating;
        double exponent = ratingDifference / 400.0;
        return 1.0 / (1.0 + Math.Pow(10, exponent));
    }

    /// <summary>
    /// Calculate rating change for a game result
    /// Formula: Rating Change = K * (Actual Score - Expected Score)
    /// </summary>
    public static int CalculateRatingChange(int playerRating, int opponentRating, GameResult result)
    {
        int kFactor = GetKFactor(playerRating);
        double expectedScore = CalculateExpectedScore(playerRating, opponentRating);
        
        double actualScore = result switch
        {
            GameResult.Win => 1.0,
            GameResult.Loss => 0.0,
            GameResult.Draw => 0.5,
            _ => 0.0
        };

        double ratingChange = kFactor * (actualScore - expectedScore);
        return (int)Math.Round(ratingChange);
    }

    /// <summary>
    /// Update player rating after a game
    /// </summary>
    public static int UpdateRating(int currentRating, int opponentRating, GameResult result)
    {
        int ratingChange = CalculateRatingChange(currentRating, opponentRating, result);
        int newRating = currentRating + ratingChange;
        
        // Ensure rating doesn't go below 0
        return Math.Max(newRating, 0);
    }

    /// <summary>
    /// Calculate rating change for multiple games
    /// </summary>
    public static int CalculateCumulativeRatingChange(int playerRating, List<(int opponentRating, GameResult result)> games)
    {
        int totalChange = 0;
        int currentRating = playerRating;

        foreach (var (opponentRating, result) in games)
        {
            int change = CalculateRatingChange(currentRating, opponentRating, result);
            totalChange += change;
            currentRating += change;
        }

        return totalChange;
    }

    /// <summary>
    /// Get rating category based on Elo rating
    /// </summary>
    public static string GetRatingCategory(int rating)
    {
        return rating switch
        {
            < 800 => "Beginner",
            < 1200 => "Novice",
            < 1400 => "Intermediate",
            < 1600 => "Advanced",
            < 1800 => "Expert",
            < 2000 => "Master",
            < 2200 => "International Master",
            < 2400 => "Grandmaster",
            _ => "Super Grandmaster"
        };
    }

    /// <summary>
    /// Get rating description with category
    /// </summary>
    public static (string category, string description) GetRatingInfo(int rating)
    {
        var category = GetRatingCategory(rating);
        var description = category switch
        {
            "Beginner" => "Just starting to learn chess",
            "Novice" => "Basic understanding of chess rules",
            "Intermediate" => "Solid chess knowledge",
            "Advanced" => "Strong player with good tactics",
            "Expert" => "Very strong player",
            "Master" => "Master level player",
            "International Master" => "Near grandmaster level",
            "Grandmaster" => "Grandmaster level",
            "Super Grandmaster" => "Elite world-class player",
            _ => "Unknown rating"
        };
        return (category, description);
    }

    /// <summary>
    /// Get rating color based on rating (for UI purposes)
    /// </summary>
    public static string GetRatingColor(int rating)
    {
        return rating switch
        {
            < 800 => "#8B4513", // Brown - Beginner
            < 1200 => "#FF0000", // Red - Novice
            < 1400 => "#FF8C00", // Orange - Intermediate
            < 1600 => "#FFD700", // Gold - Advanced
            < 1800 => "#32CD32", // Green - Expert
            < 2000 => "#1E90FF", // Blue - Master
            < 2200 => "#9932CC", // Purple - International Master
            < 2400 => "#FF1493", // Deep Pink - Grandmaster
            _ => "#FFD700"       // Gold - Super Grandmaster
        };
    }
}

/// <summary>
/// Enum for game results
/// </summary>
public enum GameResult
{
    Win = 1,
    Loss = 0,
    Draw = 2
}
