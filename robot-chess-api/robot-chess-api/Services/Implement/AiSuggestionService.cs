using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using robot_chess_api.Data;
using robot_chess_api.DTOs;
using robot_chess_api.Helpers;
using robot_chess_api.Models;
using robot_chess_api.Repositories;
using robot_chess_api.Services.Interface;
using System;
using System.Threading.Tasks;

namespace robot_chess_api.Services.Implement
{
    /// <summary>
    /// AI Suggestion Service - Simplified version
    /// ONLY deducts points from user, does NOT save to ai_suggestions table
    /// Uses in-memory cache for rate limiting (3 seconds between requests)
    /// </summary>
    public class AiSuggestionService : IAiSuggestionService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPointTransactionRepository _pointTransactionRepository;
        private readonly ChessEngineHelper _chessEngineHelper;
        private readonly PostgresContext _context;
        private readonly ILogger<AiSuggestionService> _logger;
        private readonly IMemoryCache _cache;

        // Cost in points for one AI suggestion (5 points = basic tier)
        private const int SUGGESTION_COST = 5;
        
        // Rate limit: 1 request per 3 seconds to prevent spam
        private const int RATE_LIMIT_SECONDS = 3;

        public AiSuggestionService(
            IUserRepository userRepository,
            IPointTransactionRepository pointTransactionRepository,
            ChessEngineHelper chessEngineHelper,
            PostgresContext context,
            ILogger<AiSuggestionService> logger,
            IMemoryCache cache)
        {
            _userRepository = userRepository;
            _pointTransactionRepository = pointTransactionRepository;
            _chessEngineHelper = chessEngineHelper;
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<SuggestionResponseDto> GetSuggestionAsync(Guid userId, GetSuggestionRequestDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Check rate limit (prevent spam - even if user has points)
                if (!await CheckRateLimitAsync(userId))
                {
                    throw new Exception($"Vui lòng đợi {RATE_LIMIT_SECONDS} giây trước khi yêu cầu gợi ý tiếp theo");
                }

                // 2. Validate FEN position
                if (!_chessEngineHelper.IsValidFen(request.FenPosition))
                {
                    throw new ArgumentException("FEN position không hợp lệ");
                }

                // 3. Check user exists and has enough points
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    throw new Exception("Không tìm thấy người dùng");
                }

                if (user.PointsBalance < SUGGESTION_COST)
                {
                    throw new Exception($"Không đủ điểm. Cần {SUGGESTION_COST} điểm, số dư hiện tại: {user.PointsBalance}");
                }

                // 4. Get chess analysis from Chess API (only backend can call this)
                _logger.LogInformation($"Getting chess suggestion for user {userId}, game {request.GameId}");
                var depth = request.Depth > 0 ? request.Depth : 15;
                var analysis = await _chessEngineHelper.GetBestMoveAsync(request.FenPosition, depth);

                if (string.IsNullOrEmpty(analysis.BestMove))
                {
                    throw new Exception("Không tìm thấy nước đi hợp lệ");
                }
                
                _logger.LogInformation($"Chess API returned move: {analysis.BestMove}, depth: {analysis.Depth}, eval: {analysis.Evaluation}");

                // 5. Deduct points from user
                user.PointsBalance -= SUGGESTION_COST;
                await _context.SaveChangesAsync();

                // 6. Get game info to include game type in description
                var game = await _context.Games
                    .Include(g => g.GameType)
                    .FirstOrDefaultAsync(g => g.Id == request.GameId);
                
                string gameTypeName = game?.GameType?.Name ?? "Unknown Game";
                
                // 7. Create point transaction record ONLY (không lưu vào ai_suggestions table)
                var pointTransaction = new PointTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Amount = -SUGGESTION_COST,
                    TransactionType = "service_usage",
                    Description = $"AI Hint for {gameTypeName}",
                    CreatedAt = DateTime.UtcNow
                };
                await _pointTransactionRepository.CreateAsync(pointTransaction);

                // 8. Set rate limit cache (user must wait 3 seconds)
                SetRateLimit(userId);

                await transaction.CommitAsync();

                _logger.LogInformation($"AI suggestion provided to user {userId}, new balance: {user.PointsBalance}");

                // 9. Return response (suggestion ID is temporary, not saved to DB)
                return new SuggestionResponseDto
                {
                    SuggestionId = Guid.NewGuid(), // Temporary ID for frontend reference
                    SuggestedMove = analysis.BestMove,
                    SuggestedMoveSan = analysis.San, // Use San directly from Chess API response
                    Evaluation = analysis.Evaluation,
                    Confidence = analysis.Confidence,
                    BestLine = analysis.BestLine,
                    PointsDeducted = SUGGESTION_COST,
                    RemainingPoints = user.PointsBalance,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error getting suggestion for user {userId}");
                throw;
            }
        }

        /// <summary>
        /// Check if user is rate limited (prevent spam)
        /// Uses in-memory cache to track last request time
        /// </summary>
        public async Task<bool> CheckRateLimitAsync(Guid userId)
        {
            var cacheKey = $"ai_suggest_ratelimit_{userId}";
            
            // If key exists in cache = user is rate limited
            if (_cache.TryGetValue(cacheKey, out _))
            {
                _logger.LogWarning($"User {userId} is rate limited for AI suggestions");
                return false;
            }

            return await Task.FromResult(true);
        }

        /// <summary>
        /// Set rate limit for user (cache expires after RATE_LIMIT_SECONDS)
        /// </summary>
        private void SetRateLimit(Guid userId)
        {
            var cacheKey = $"ai_suggest_ratelimit_{userId}";
            _cache.Set(cacheKey, true, TimeSpan.FromSeconds(RATE_LIMIT_SECONDS));
        }

        public int GetSuggestionCost()
        {
            return SUGGESTION_COST;
        }
    }
}
