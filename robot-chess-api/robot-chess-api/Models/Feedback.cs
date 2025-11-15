using System;
using System.Collections.Generic;

namespace robot_chess_api.Models;

public partial class Feedback
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public string? Message { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AppUser? User { get; set; }
}
