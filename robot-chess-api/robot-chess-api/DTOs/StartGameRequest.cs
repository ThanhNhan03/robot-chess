using System.ComponentModel.DataAnnotations;

namespace robot_chess_api.DTOs
{
    public class StartGameRequest
    {
        [Required]
        public string Difficulty { get; set; } = "medium"; // easy, medium, hard
    }
}
