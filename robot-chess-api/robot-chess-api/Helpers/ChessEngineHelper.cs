using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace robot_chess_api.Helpers
{
    /// <summary>
    /// Helper class for chess engine integration
    /// Uses Chess API (chess-api.com) powered by Stockfish 17 for chess analysis
    /// </summary>
    public class ChessEngineHelper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ChessEngineHelper> _logger;

        public ChessEngineHelper(HttpClient httpClient, ILogger<ChessEngineHelper> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Get best move suggestion from Chess API (chess-api.com)
        /// </summary>
        /// <param name="fenPosition">FEN notation of current position</param>
        /// <param name="depth">Analysis depth (12-18 recommended, max 18)</param>
        /// <returns>Chess analysis result</returns>
        public async Task<ChessAnalysisResult> GetBestMoveAsync(string fenPosition, int depth = 15)
        {
            try
            {
                // Chess API endpoint (Stockfish 17)
                var url = "https://chess-api.com/v1";
                
                _logger.LogInformation($"Requesting chess analysis for FEN: {fenPosition}, depth: {depth}");

                // Prepare request body
                var requestBody = new
                {
                    fen = fenPosition,
                    depth = Math.Min(depth, 18), // Max depth is 18
                    variants = 1 // Only need best move
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, httpContent);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Chess API error: {response.StatusCode}");
                    throw new Exception($"Chess engine API returned {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ChessApiResponse>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (result == null || string.IsNullOrEmpty(result.Move))
                {
                    _logger.LogWarning("No analysis available for this position");
                    throw new Exception("No analysis available for this position");
                }

                // Convert centipawns string to int
                int? evaluation = null;
                if (!string.IsNullOrEmpty(result.Centipawns))
                {
                    if (int.TryParse(result.Centipawns, out int cp))
                    {
                        evaluation = cp;
                    }
                }
                
                return new ChessAnalysisResult
                {
                    BestMove = result.Move ?? "",
                    San = result.San ?? result.Move ?? "",
                    Evaluation = evaluation,
                    Mate = result.Mate,
                    Depth = result.Depth,
                    BestLine = result.ContinuationArr ?? new List<string>(),
                    Confidence = CalculateConfidence(evaluation, result.Mate, result.Depth)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting chess analysis");
                throw;
            }
        }

        /// <summary>
        /// Validate FEN notation
        /// </summary>
        public bool IsValidFen(string fen)
        {
            if (string.IsNullOrWhiteSpace(fen))
                return false;

            var parts = fen.Split(' ');
            if (parts.Length < 4)
                return false;

            // Basic FEN validation
            var ranks = parts[0].Split('/');
            if (ranks.Length != 8)
                return false;

            // Turn indicator
            if (parts[1] != "w" && parts[1] != "b")
                return false;

            return true;
        }

        /// <summary>
        /// Calculate confidence level based on evaluation and depth
        /// </summary>
        private decimal CalculateConfidence(int? centipawns, int? mate, int depth)
        {
            // Mate found = highest confidence
            if (mate.HasValue)
                return 1.0m;

            // Base confidence on depth
            decimal baseConfidence = Math.Min((decimal)depth / 20m, 1.0m);

            // Adjust based on evaluation clarity
            if (centipawns.HasValue)
            {
                var absEval = Math.Abs(centipawns.Value);
                
                // Clear advantage = higher confidence
                if (absEval > 300) // > 3 pawns advantage
                    baseConfidence = Math.Min(baseConfidence + 0.2m, 1.0m);
                else if (absEval < 50) // Nearly equal position
                    baseConfidence = Math.Max(baseConfidence - 0.1m, 0.5m);
            }

            return Math.Round(baseConfidence, 2);
        }

        /// <summary>
        /// Convert UCI move to SAN (Standard Algebraic Notation)
        /// Note: This is deprecated - use analysis.San directly from GetBestMoveAsync result
        /// </summary>
        [Obsolete("Use analysis.San directly from GetBestMoveAsync result")]
        public string UciToSan(string uciMove, string fenPosition)
        {
            // Chess API provides SAN directly in response
            // This method is kept for backward compatibility
            return uciMove;
        }
    }

    #region Response Models

    public class ChessAnalysisResult
    {
        public string BestMove { get; set; } = string.Empty;
        public string San { get; set; } = string.Empty;
        public int? Evaluation { get; set; }
        public int? Mate { get; set; }
        public int Depth { get; set; }
        public List<string> BestLine { get; set; } = new List<string>();
        public decimal Confidence { get; set; }
    }

    /// <summary>
    /// Chess API response model (chess-api.com)
    /// </summary>
    public class ChessApiResponse
    {
        public string? Text { get; set; }
        public double? Eval { get; set; }
        public string? Move { get; set; }
        public string? Fen { get; set; }
        public int Depth { get; set; }
        public double? WinChance { get; set; }
        public List<string>? ContinuationArr { get; set; }
        public int? Mate { get; set; }
        public string? Centipawns { get; set; }
        public string? San { get; set; }
        public string? Lan { get; set; }
        public string? Turn { get; set; }
        public string? Color { get; set; }
        public string? Piece { get; set; }
        public string? Flags { get; set; }
        public bool IsCapture { get; set; }
        public bool IsCastling { get; set; }
        public bool IsPromotion { get; set; }
        public string? From { get; set; }
        public string? To { get; set; }
        public string? FromNumeric { get; set; }
        public string? ToNumeric { get; set; }
        public string? TaskId { get; set; }
        public int? Time { get; set; }
        public string? Type { get; set; }
    }

    #endregion
}
