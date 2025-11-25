using System.ComponentModel.DataAnnotations;

namespace robot_chess_api.DTOs;

public class FaqDto
{
    public Guid Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsPublished { get; set; }
    public int? DisplayOrder { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateFaqDto
{
    [Required(ErrorMessage = "Question is required")]
    [MaxLength(500, ErrorMessage = "Question cannot exceed 500 characters")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "Answer is required")]
    public string Answer { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string? Category { get; set; }

    public bool IsPublished { get; set; } = false;

    public int? DisplayOrder { get; set; }
}

public class UpdateFaqDto
{
    [MaxLength(500, ErrorMessage = "Question cannot exceed 500 characters")]
    public string? Question { get; set; }

    public string? Answer { get; set; }

    [MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string? Category { get; set; }

    public bool? IsPublished { get; set; }

    public int? DisplayOrder { get; set; }
}
