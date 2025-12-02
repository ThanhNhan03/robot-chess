using robot_chess_api.DTOs;
using robot_chess_api.Models;
using robot_chess_api.Repositories;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Services.Implement
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IGameMoveRepository _gameMoveRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<GameService> _logger;
        private readonly string _tcpServerUrl = "http://localhost:5000";

        public GameService(
            IGameRepository gameRepository,
            IGameMoveRepository gameMoveRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<GameService> logger)
        {
            _gameRepository = gameRepository;
            _gameMoveRepository = gameMoveRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<GameTypeDto>> GetAllGameTypesAsync()
        {
            var gameTypes = await _gameRepository.GetAllGameTypesAsync();
            return gameTypes.Select(gt => new GameTypeDto
            {
                Id = gt.Id,
                Code = gt.Code,
                Name = gt.Name,
                Description = gt.Description,
                CreatedAt = gt.CreatedAt
            });
        }

        public async Task<StartGameResponseDto> StartGameAsync(StartGameRequestDto request, Guid? playerId)
        {
            // Validate game type
            var gameType = await _gameRepository.GetGameTypeByCodeAsync(request.GameTypeCode);
            if (gameType == null)
            {
                throw new ArgumentException($"Game type '{request.GameTypeCode}' not found");
            }

            // Validate puzzle if training_puzzle
            if (request.GameTypeCode == "training_puzzle" && !request.PuzzleId.HasValue)
            {
                throw new ArgumentException("Puzzle ID is required for training puzzle mode");
            }

            // Create new game record
            var game = new Game
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                GameTypeId = gameType.Id,
                PuzzleId = request.PuzzleId,
                Difficulty = request.Difficulty,
                Status = "waiting",
                FenStart = "5r1k/1b2Nppp/8/2R5/4Q3/8/5PPP/6K1 w - - 0 1",
                TotalMoves = 0,
                CreatedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow
            };

            await _gameRepository.CreateAsync(game);

            // Send start game command to AI via TCP Server
            var requestId = Guid.NewGuid();
            try
            {
                var aiMessage = new
                {
                    type = "ai_request",
                    request_id = requestId.ToString(),
                    command = "start_game",
                    payload = new
                    {
                        game_id = game.Id.ToString(),
                        status = "start",
                        difficulty = request.Difficulty,
                        game_type = request.GameTypeCode,
                        puzzle_id = request.PuzzleId?.ToString()
                    }
                };

                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.PostAsJsonAsync($"{_tcpServerUrl}/internal/ai-command", aiMessage);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to send start game command to TCP Server: {response.StatusCode}");
                }
                else
                {
                    _logger.LogInformation($"Start game command sent successfully. Game ID: {game.Id}");
                    
                    // Update game status to in_progress
                    game.Status = "in_progress";
                    await _gameRepository.UpdateAsync(game);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending start game command to TCP Server");
            }

            return new StartGameResponseDto
            {
                GameId = game.Id,
                RequestId = requestId,
                GameTypeCode = request.GameTypeCode,
                Difficulty = request.Difficulty,
                Status = game.Status ?? "waiting",
                Message = "Game started successfully",
                FenStart = game.FenStart
            };
        }

        public async Task<StartGameResponseDto> ResumeGameAsync(ResumeGameRequestDto request)
        {
            var game = await _gameRepository.GetByIdAsync(request.GameId);
            if (game == null)
            {
                throw new ArgumentException($"Game with ID {request.GameId} not found");
            }

            if (game.Status == "finished")
            {
                throw new InvalidOperationException("Cannot resume a finished game");
            }

            // Send resume game command to AI via TCP Server
            var requestId = Guid.NewGuid();
            try
            {
                var aiMessage = new
                {
                    type = "ai_request",
                    request_id = requestId.ToString(),
                    command = "resume_game",
                    payload = new
                    {
                        game_id = game.Id.ToString(),
                        status = "resume",
                        difficulty = game.Difficulty ?? "medium",
                        fen = game.FenCurrent ?? game.FenStart ?? "5r1k/1b2Nppp/8/2R5/4Q3/8/5PPP/6K1 w - - 0 1"
                    }
                };

                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.PostAsJsonAsync($"{_tcpServerUrl}/internal/ai-command", aiMessage);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to send resume game command to TCP Server: {response.StatusCode}");
                }
                else
                {
                    _logger.LogInformation($"Resume game command sent successfully. Game ID: {game.Id}");
                    
                    // Update game status to in_progress
                    game.Status = "in_progress";
                    await _gameRepository.UpdateAsync(game);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending resume game command to TCP Server");
            }

            return new StartGameResponseDto
            {
                GameId = game.Id,
                RequestId = requestId,
                GameTypeCode = game.GameType?.Code ?? "normal_game",
                Difficulty = game.Difficulty ?? "medium",
                Status = game.Status ?? "in_progress",
                Message = "Game resumed successfully",
                FenStart = game.FenCurrent ?? game.FenStart
            };
        }

        public async Task<GameDto?> GetGameByIdAsync(Guid id)
        {
            var game = await _gameRepository.GetByIdAsync(id);
            if (game == null) return null;

            return new GameDto
            {
                Id = game.Id,
                PlayerId = game.PlayerId,
                Status = game.Status,
                Result = game.Result,
                FenStart = game.FenStart,
                FenCurrent = game.FenCurrent,
                TotalMoves = game.TotalMoves,
                StartedAt = game.StartedAt,
                EndedAt = game.EndedAt,
                CreatedAt = game.CreatedAt,
                GameTypeId = game.GameTypeId,
                PuzzleId = game.PuzzleId,
                Difficulty = game.Difficulty,
                GameType = game.GameType != null ? new GameTypeDto
                {
                    Id = game.GameType.Id,
                    Code = game.GameType.Code,
                    Name = game.GameType.Name,
                    Description = game.GameType.Description
                } : null,
                PlayerName = game.Player?.FullName ?? game.Player?.Username
            };
        }

        public async Task<IEnumerable<GameDto>> GetPlayerGamesAsync(Guid playerId)
        {
            var games = await _gameRepository.GetByPlayerIdAsync(playerId);
            return games.Select(game => new GameDto
            {
                Id = game.Id,
                PlayerId = game.PlayerId,
                Status = game.Status,
                Result = game.Result,
                FenStart = game.FenStart,
                FenCurrent = game.FenCurrent,
                TotalMoves = game.TotalMoves,
                StartedAt = game.StartedAt,
                EndedAt = game.EndedAt,
                CreatedAt = game.CreatedAt,
                GameTypeId = game.GameTypeId,
                PuzzleId = game.PuzzleId,
                Difficulty = game.Difficulty,
                GameType = game.GameType != null ? new GameTypeDto
                {
                    Id = game.GameType.Id,
                    Code = game.GameType.Code,
                    Name = game.GameType.Name,
                    Description = game.GameType.Description
                } : null
            });
        }

        public async Task<BoardSetupStatusDto> VerifyBoardSetupAsync(Guid gameId)
        {
            // Get game to verify it exists
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
            {
                throw new ArgumentException($"Game with ID {gameId} not found");
            }

            // Send verify board setup command to AI via TCP Server
            var requestId = Guid.NewGuid();
            try
            {
                var aiMessage = new
                {
                    type = "ai_request",
                    request_id = requestId.ToString(),
                    command = "verify_board_setup",
                    payload = new
                    {
                        game_id = gameId.ToString()
                    }
                };

                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.PostAsJsonAsync($"{_tcpServerUrl}/internal/ai-command", aiMessage);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to send verify board setup command to TCP Server: {response.StatusCode}");
                    
                    return new BoardSetupStatusDto
                    {
                        GameId = gameId,
                        Status = "unknown",
                        Message = "Failed to communicate with AI service",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else
                {
                    _logger.LogInformation($"Verify board setup command sent successfully. Game ID: {gameId}");
                    
                    // Note: The actual status will be received via WebSocket from TCP Server
                    // This endpoint just triggers the verification
                    return new BoardSetupStatusDto
                    {
                        GameId = gameId,
                        Status = "pending",
                        Message = "Board setup verification requested - status will be sent via WebSocket",
                        Timestamp = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending verify board setup command to TCP Server");
                
                return new BoardSetupStatusDto
                {
                    GameId = gameId,
                    Status = "error",
                    Message = $"Error requesting verification: {ex.Message}",
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        public async Task<GameMoveDto> SaveMoveAsync(CreateGameMoveDto moveDto)
        {
            // Validate game exists
            var game = await _gameRepository.GetByIdAsync(moveDto.GameId);
            if (game == null)
            {
                throw new ArgumentException($"Game with ID {moveDto.GameId} not found");
            }

            // Create game move entity
            var move = new GameMove
            {
                Id = Guid.NewGuid(),
                GameId = moveDto.GameId,
                MoveNumber = moveDto.MoveNumber,
                PlayerColor = moveDto.PlayerColor,
                FromSquare = moveDto.FromSquare,
                ToSquare = moveDto.ToSquare,
                FromPiece = moveDto.FromPiece,
                ToPiece = moveDto.ToPiece,
                Notation = moveDto.Notation,
                ResultsInCheck = moveDto.ResultsInCheck,
                FenStr = moveDto.FenStr,
                CreatedAt = DateTime.UtcNow
            };

            await _gameMoveRepository.CreateAsync(move);

            // Update game's current FEN and total moves
            game.FenCurrent = moveDto.FenStr;
            game.TotalMoves = moveDto.MoveNumber;
            await _gameRepository.UpdateAsync(game);

            _logger.LogInformation($"Saved move {moveDto.MoveNumber} for game {moveDto.GameId}: {moveDto.Notation}");

            return new GameMoveDto
            {
                Id = move.Id,
                GameId = move.GameId ?? Guid.Empty,
                MoveNumber = move.MoveNumber ?? 0,
                PlayerColor = move.PlayerColor ?? "",
                FromSquare = move.FromSquare ?? "",
                ToSquare = move.ToSquare ?? "",
                FromPiece = move.FromPiece,
                ToPiece = move.ToPiece,
                Notation = move.Notation ?? "",
                ResultsInCheck = move.ResultsInCheck ?? false,
                FenStr = move.FenStr ?? "",
                CreatedAt = move.CreatedAt ?? DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<GameMoveDto>> SaveMovesAsync(SaveMovesRequestDto request)
        {
            // Validate game exists
            var game = await _gameRepository.GetByIdAsync(request.GameId);
            if (game == null)
            {
                throw new ArgumentException($"Game with ID {request.GameId} not found");
            }

            var moves = request.Moves.Select(moveDto => new GameMove
            {
                Id = Guid.NewGuid(),
                GameId = moveDto.GameId,
                MoveNumber = moveDto.MoveNumber,
                PlayerColor = moveDto.PlayerColor,
                FromSquare = moveDto.FromSquare,
                ToSquare = moveDto.ToSquare,
                FromPiece = moveDto.FromPiece,
                ToPiece = moveDto.ToPiece,
                Notation = moveDto.Notation,
                ResultsInCheck = moveDto.ResultsInCheck,
                FenStr = moveDto.FenStr,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _gameMoveRepository.CreateManyAsync(moves);

            // Update game with latest FEN and total moves
            if (moves.Any())
            {
                var latestMove = moves.OrderByDescending(m => m.MoveNumber).First();
                game.FenCurrent = latestMove.FenStr;
                game.TotalMoves = latestMove.MoveNumber;
                await _gameRepository.UpdateAsync(game);
            }

            _logger.LogInformation($"Saved {moves.Count} moves for game {request.GameId}");

            return moves.Select(m => new GameMoveDto
            {
                Id = m.Id,
                GameId = m.GameId ?? Guid.Empty,
                MoveNumber = m.MoveNumber ?? 0,
                PlayerColor = m.PlayerColor ?? "",
                FromSquare = m.FromSquare ?? "",
                ToSquare = m.ToSquare ?? "",
                FromPiece = m.FromPiece,
                ToPiece = m.ToPiece,
                Notation = m.Notation ?? "",
                ResultsInCheck = m.ResultsInCheck ?? false,
                FenStr = m.FenStr ?? "",
                CreatedAt = m.CreatedAt ?? DateTime.UtcNow
            });
        }

        public async Task<IEnumerable<GameMoveDto>> GetGameMovesAsync(Guid gameId)
        {
            var moves = await _gameMoveRepository.GetByGameIdAsync(gameId);
            return moves.Select(m => new GameMoveDto
            {
                Id = m.Id,
                GameId = m.GameId ?? Guid.Empty,
                MoveNumber = m.MoveNumber ?? 0,
                PlayerColor = m.PlayerColor ?? "",
                FromSquare = m.FromSquare ?? "",
                ToSquare = m.ToSquare ?? "",
                FromPiece = m.FromPiece,
                ToPiece = m.ToPiece,
                Notation = m.Notation ?? "",
                ResultsInCheck = m.ResultsInCheck ?? false,
                FenStr = m.FenStr ?? "",
                CreatedAt = m.CreatedAt ?? DateTime.UtcNow
            });
        }

        public async Task<IEnumerable<GameMoveDto>> GetGameMovesRangeAsync(GetMovesRequestDto request)
        {
            var moves = await _gameMoveRepository.GetByGameIdRangeAsync(
                request.GameId,
                request.FromMoveNumber,
                request.ToMoveNumber);

            return moves.Select(m => new GameMoveDto
            {
                Id = m.Id,
                GameId = m.GameId ?? Guid.Empty,
                MoveNumber = m.MoveNumber ?? 0,
                PlayerColor = m.PlayerColor ?? "",
                FromSquare = m.FromSquare ?? "",
                ToSquare = m.ToSquare ?? "",
                FromPiece = m.FromPiece,
                ToPiece = m.ToPiece,
                Notation = m.Notation ?? "",
                ResultsInCheck = m.ResultsInCheck ?? false,
                FenStr = m.FenStr ?? "",
                CreatedAt = m.CreatedAt ?? DateTime.UtcNow
            });
        }

        public async Task<UpdateGameResultResponseDto> UpdateGameResultAsync(UpdateGameResultRequestDto request)
        {
            // Validate result value
            var validResults = new[] { "win", "lose", "draw", "abandoned" };
            if (!validResults.Contains(request.Result.ToLower()))
            {
                throw new ArgumentException($"Invalid result value. Must be one of: {string.Join(", ", validResults)}");
            }

            // Get existing game
            var game = await _gameRepository.GetByIdAsync(request.GameId);
            if (game == null)
            {
                throw new ArgumentException($"Game with ID {request.GameId} not found");
            }

            // Update game properties
            game.Result = request.Result.ToLower();
            // Map status to database constraint values: 'waiting', 'in_progress', 'finished'
            game.Status = request.Status == "completed" ? "finished" : request.Status;
            game.EndedAt = DateTime.UtcNow;

            // Update FEN if provided
            if (!string.IsNullOrEmpty(request.FenCurrent))
            {
                game.FenCurrent = request.FenCurrent;
            }

            // Update total moves if provided, otherwise keep existing or calculate from moves
            if (request.TotalMoves.HasValue)
            {
                game.TotalMoves = request.TotalMoves.Value;
            }
            else if (!game.TotalMoves.HasValue || game.TotalMoves == 0)
            {
                // Try to get from database moves
                var moves = await _gameMoveRepository.GetByGameIdAsync(request.GameId);
                if (moves.Any())
                {
                    game.TotalMoves = moves.Max(m => m.MoveNumber ?? 0);
                }
            }

            await _gameRepository.UpdateAsync(game);

            _logger.LogInformation($"Game {request.GameId} updated - Result: {game.Result}, Status: {game.Status}, Total Moves: {game.TotalMoves}");

            // Send end command to AI via TCP Server
            try
            {
                var aiMessage = new
                {
                    Type = "ai_request",
                    Command = "start_game",
                    Payload = new
                    {
                        game_id = request.GameId.ToString(),
                        status = "end"
                    }
                };

                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.PostAsJsonAsync($"{_tcpServerUrl}/internal/ai-command", aiMessage);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"End game command sent to AI for game {request.GameId}");
                }
                else
                {
                    _logger.LogWarning($"Failed to send end game command to AI: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending end game command to AI for game {request.GameId}");
                // Don't throw - game result was saved successfully
            }

            return new UpdateGameResultResponseDto
            {
                GameId = game.Id,
                Result = game.Result ?? "",
                Status = game.Status ?? "",
                TotalMoves = game.TotalMoves,
                EndedAt = game.EndedAt,
                Message = $"Game result updated successfully to '{game.Result}'"
            };
        }

        public async Task<EndGameResponseDto> EndGameAsync(Guid gameId, string reason)
        {
            // Validate game exists
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
            {
                throw new ArgumentException($"Game with ID {gameId} not found");
            }

            // Send end command to AI via TCP Server
            var requestId = Guid.NewGuid();
            try
            {
                var aiMessage = new
                {
                    Type = "ai_request",
                    Command = "start_game",
                    Payload = new
                    {
                        game_id = gameId.ToString(),
                        status = "end"
                    }
                };

                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.PostAsJsonAsync($"{_tcpServerUrl}/internal/ai-command", aiMessage);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to send end game command to TCP Server: {response.StatusCode}");
                    return new EndGameResponseDto
                    {
                        GameId = gameId,
                        RequestId = requestId,
                        Status = "error",
                        Message = "Failed to communicate with AI service",
                        Reason = reason
                    };
                }

                _logger.LogInformation($"End game command sent successfully. Game ID: {gameId}, Reason: {reason}");

                return new EndGameResponseDto
                {
                    GameId = gameId,
                    RequestId = requestId,
                    Status = "sent",
                    Message = "End game command sent to AI successfully",
                    Reason = reason
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending end game command to TCP Server for game {gameId}");
                return new EndGameResponseDto
                {
                    GameId = gameId,
                    RequestId = requestId,
                    Status = "error",
                    Message = $"Error: {ex.Message}",
                    Reason = reason
                };
            }
        }
    }
}
