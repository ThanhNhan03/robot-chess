using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.Models;

namespace robot_chess_api.Repositories;

public class PointTransactionRepository : IPointTransactionRepository
{
    private readonly PostgresContext _context;

    public PointTransactionRepository(PostgresContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PointTransaction>> GetAllAsync(
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        string? transactionType = null)
    {
        var query = _context.PointTransactions
            .AsNoTracking()
            .Select(t => new PointTransaction
            {
                Id = t.Id,
                UserId = t.UserId,
                Amount = t.Amount,
                TransactionType = t.TransactionType,
                Description = t.Description,
                RelatedPaymentId = t.RelatedPaymentId,
                CreatedAt = t.CreatedAt,
                User = t.User != null ? new AppUser
                {
                    Id = t.User.Id,
                    Username = t.User.Username,
                    Email = t.User.Email,
                    FullName = t.User.FullName
                } : null
            })
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        }

        if (!string.IsNullOrEmpty(transactionType))
        {
            query = query.Where(t => t.TransactionType == transactionType);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<PointTransaction>> GetByUserIdAsync(Guid userId)
    {
        return await _context.PointTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<PointTransaction> CreateAsync(PointTransaction transaction)
    {
        _context.PointTransactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Dictionary<string, int>> GetTransactionStatisticsAsync(
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        var query = _context.PointTransactions.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        }

        var statistics = await query
            .GroupBy(t => t.TransactionType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        return statistics;
    }
}
