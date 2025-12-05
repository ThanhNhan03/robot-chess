using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace robot_chess_api.Helpers
{
    /// <summary>
    /// Helper class for chess engine integration
    /// Supports Lichess Cloud Eval API for chess analysis
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
        /// Get best move suggestion from Lichess Cloud Eval API
        /// </summary>
        /// <param name="fenPosition">FEN notation of current position</param>
        /// <param name="depth">Analysis depth (10-20 recommended)</param>
        /// <returns>Chess analysis result</returns>
        public async Task<ChessAnalysisResult> GetBestMoveAsync(string fenPosition, int depth = 15)
        {
            try
            {
                // Lichess Cloud Eval API endpoint
                var url = $"https://lichess.org/api/cloud-eval?fen={Uri.EscapeDataString(fenPosition)}&multiPv=1";
                
                _logger.LogInformation($"Requesting chess analysis for FEN: {fenPosition}");

                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Lichess API error: {response.StatusCode}");
                    throw new Exception($"Chess engine API returned {response.StatusCode}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LichessCloudEvalResponse>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (result?.Pvs == null || result.Pvs.Count == 0)
                {
                    _logger.LogWarning("No analysis available for this position");
                    throw new Exception("No analysis available for this position");
                }

                var pv = result.Pvs[0];
                
                return new ChessAnalysisResult
                {
                    BestMove = pv.Moves?.Split(' ')[0] ?? "",
                    Evaluation = pv.Cp,
                    Mate = pv.Mate,
                    Depth = result.Depth,
                    BestLine = pv.Moves?.Split(' ').ToList() ?? new List<string>(),
                    Confidence = CalculateConfidence(pv.Cp, pv.Mate, result.Depth)
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
        /// Note: This is a simplified version. For production, use a chess library
        /// </summary>
        public string UciToSan(string uciMove, string fenPosition)
        {
            // Simplified conversion - in production, use chess.js or similar library
            // For now, return UCI format
            // TODO: Implement proper UCI to SAN conversion
            return uciMove;
        }
    }

    #region Response Models

    public class ChessAnalysisResult
    {
        public string BestMove { get; set; } = string.Empty;
        public int? Evaluation { get; set; }
        public int? Mate { get; set; }
        public int Depth { get; set; }
        public List<string> BestLine { get; set; } = new List<string>();
        public decimal Confidence { get; set; }
    }

    public class LichessCloudEvalResponse
    {
        public int Depth { get; set; }
        public List<PvData> Pvs { get; set; } = new List<PvData>();
        public int? KNodes { get; set; }
    }

    public class PvData
    {
        public string? Moves { get; set; }
        public int? Cp { get; set; }  // Centipawns
        public int? Mate { get; set; } // Mate in X moves
    }

    #endregion
}
