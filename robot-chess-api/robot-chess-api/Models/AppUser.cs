using System;
using System.Collections.Generic;

namespace robot_chess_api.Models;

public partial class AppUser
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string? FullName { get; set; }

    public string? AvatarUrl { get; set; }

    // New properties for user management
    public string Role { get; set; } = "player"; // admin, player, viewer

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Game> Games { get; set; } = new List<Game>();

    public virtual ICollection<PaymentHistory> PaymentHistories { get; set; } = new List<PaymentHistory>();

    public virtual ICollection<SavedState> SavedStates { get; set; } = new List<SavedState>();
    
    public virtual ICollection<RobotCommandHistory> RobotCommandHistories { get; set; } = new List<RobotCommandHistory>();
}
