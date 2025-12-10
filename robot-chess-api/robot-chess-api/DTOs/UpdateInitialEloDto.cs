using System.ComponentModel.DataAnnotations;

namespace robot_chess_api.DTOs;

/// <summary>
/// DTO for updating initial Elo rating
/// </summary>
public class UpdateInitialEloDto
{
    [Required]
    [Range(0, 3000, ErrorMessage = "Elo rating must be between 0 and 3000")]
    public int EloRating { get; set; }
}
