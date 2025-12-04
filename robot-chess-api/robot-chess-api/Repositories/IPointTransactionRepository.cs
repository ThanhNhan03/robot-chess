using robot_chess_api.Models;

namespace robot_chess_api.Repositories;

public interface IPointTransactionRepository
{
    /// <summary>
    /// Get all point transactions with optional filtering
    /// </summary>
    Task<IEnumerable<PointTransaction>> GetAllAsync(
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        string? transactionType = null);
    
    /// <summary>
    /// Get point transactions by user ID
    /// </summary>
    Task<IEnumerable<PointTransaction>> GetByUserIdAsync(Guid userId);
    
    /// <summary>
    /// Create new point transaction record
    /// </summary>
    Task<PointTransaction> CreateAsync(PointTransaction transaction);
    
    /// <summary>
    /// Get transaction statistics by type
    /// </summary>
    Task<Dictionary<string, int>> GetTransactionStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
}
