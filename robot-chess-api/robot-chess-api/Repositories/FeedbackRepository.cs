using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.Models;

namespace robot_chess_api.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly PostgresContext _context;

    public FeedbackRepository(PostgresContext context)
    {
        _context = context;
    }

    public async Task<List<Feedback>> GetAllFeedbacksAsync()
    {
        return await _context.Feedbacks
            .Include(f => f.User)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<Feedback?> GetFeedbackByIdAsync(Guid id)
    {
        return await _context.Feedbacks
            .Include(f => f.User)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<List<Feedback>> GetFeedbacksByUserIdAsync(Guid userId)
    {
        return await _context.Feedbacks
            .Include(f => f.User)
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<Feedback> CreateFeedbackAsync(Feedback feedback)
    {
        feedback.CreatedAt = DateTime.UtcNow;
        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();
        
        // Load the User relation after save
        await _context.Entry(feedback).Reference(f => f.User).LoadAsync();
        
        return feedback;
    }

    public async Task<Feedback> UpdateFeedbackAsync(Feedback feedback)
    {
        _context.Feedbacks.Update(feedback);
        await _context.SaveChangesAsync();
        
        // Reload with User relation
        await _context.Entry(feedback).Reference(f => f.User).LoadAsync();
        
        return feedback;
    }

    public async Task<bool> DeleteFeedbackAsync(Guid id)
    {
        var feedback = await _context.Feedbacks.FindAsync(id);
        if (feedback == null) return false;

        _context.Feedbacks.Remove(feedback);
        await _context.SaveChangesAsync();
        return true;
    }
}
