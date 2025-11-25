using System;
using System.Collections.Generic;

namespace robot_chess_api.Models;

public partial class Faq
{
    public Guid Id { get; set; }

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;

    public string? Category { get; set; }

    public bool IsPublished { get; set; }

    public int? DisplayOrder { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
