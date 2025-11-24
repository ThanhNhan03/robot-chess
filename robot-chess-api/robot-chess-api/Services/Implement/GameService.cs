using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.DTOs;
using robot_chess_api.Models;
using robot_chess_api.Services.Interface;
using System.Text.Json;

namespace robot_chess_api.Services.Implement
{
    public class GameService : IGameService
    {
        private readonly PostgresContext _context;
        private readonly ILogger<GameService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public GameService(PostgresContext context, ILogger<GameService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<GameDto> StartGameAsync(StartGameRequest request, Guid? userId)
        {
            // 1. Create new Game entity
            var newGame = new Game
            {
                Id = Guid.NewGuid(),
                PlayerId = userId,
                Status = "in_progress",
                Result = null,
                FenStart = "startpos",
                FenCurrent = "startpos",
                TotalMoves = 0,
                StartedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
                // Note: GameTypeId and PuzzleId are nullable and omitted for now
            };

            _context.Games.Add(newGame);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created new game {newGame.Id} for user {userId}");

            // 2. Send Start Game command to TCP Server (to forward to AI)
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var commandPayload = new
                {
                    CommandId = Guid.NewGuid(),
                    RobotId = "system", // Not specific to a robot initially
                    CommandType = "start_game",
                    Payload = new
                    {
                        game_id = newGame.Id,
                        difficulty = request.Difficulty,
                        player_color = "white",
                        fen = newGame.FenStart
                    }
                };

                var tcpServerUrl = _configuration["TcpServerUrl"] ?? "http://localhost:5005";
                var response = await httpClient.PostAsJsonAsync($"{tcpServerUrl}/internal/command", commandPayload);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to send start_game command to TCP Server: {response.StatusCode}");
                }
                else
                {
                    _logger.LogInformation($"Sent start_game command to TCP Server for game {newGame.Id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error communicating with TCP Server");
            }

            // 3. Return DTO
            return new GameDto
            {
                Id = newGame.Id,
                Status = newGame.Status,
                Result = newGame.Result ?? string.Empty,
                FenCurrent = newGame.FenCurrent ?? "startpos",
                TotalMoves = newGame.TotalMoves ?? 0,
                StartedAt = newGame.StartedAt,
                Difficulty = request.Difficulty
            };
        }

        public async Task<bool> StopGameAsync(Guid gameId)
        {
            // 1. Get Game
            var game = await _context.Games.FindAsync(gameId);
            if (game == null)
            {
                return false;
            }

            // 2. Check Status
            if (game.Status == "finished")
            {
                return true;
            }

            string currentFen = game.FenCurrent ?? "startpos";

            // 3. Send Stop Command to TCP Server and get FEN
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var commandPayload = new
                {
                    CommandId = Guid.NewGuid(),
                    RobotId = "system",
                    CommandType = "stop_game",
                    Payload = new
                    {
                        game_id = game.Id
                    }
                };

                var tcpServerUrl = _configuration["TcpServerUrl"] ?? "http://localhost:5005";
                var response = await httpClient.PostAsJsonAsync($"{tcpServerUrl}/internal/command", commandPayload);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(content);
                    if (jsonDoc.RootElement.TryGetProperty("fen", out var fenProp) && fenProp.ValueKind == JsonValueKind.String)
                    {
                        var retrievedFen = fenProp.GetString();
                        if (!string.IsNullOrEmpty(retrievedFen))
                        {
                            currentFen = retrievedFen;
                            _logger.LogInformation($"Retrieved FEN from bot: {currentFen}");
                        }
                    }
                    _logger.LogInformation($"Sent stop_game command to TCP Server for game {game.Id}");
                }
                else
                {
                    _logger.LogWarning($"Failed to send stop_game command to TCP Server: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error communicating with TCP Server during StopGame");
            }

            // 4. Save state before stopping
            var savedState = new SavedState
            {
                Id = Guid.NewGuid(),
                GameId = game.Id,
                PlayerId = game.PlayerId,
                FenStr = currentFen,
                SavedAt = DateTime.UtcNow
            };
            _context.SavedStates.Add(savedState);

            // 5. Update game status
            game.Status = "finished"; 
            game.Result = null; 
            game.EndedAt = DateTime.UtcNow;
            game.FenCurrent = currentFen; // Update with latest FEN
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Game {gameId} stopped and state saved. ID: {savedState.Id}");

            return true;
        }

        public async Task<GameDto?> GetGameAsync(Guid gameId)
        {
            var game = await _context.Games.FindAsync(gameId);
            if (game == null) return null;

            return new GameDto
            {
                Id = game.Id,
                Status = game.Status,
                Result = game.Result ?? string.Empty,
                FenCurrent = game.FenCurrent ?? "startpos",
                TotalMoves = game.TotalMoves ?? 0,
                StartedAt = game.StartedAt,
                EndedAt = game.EndedAt
            };
        }
    }
}
