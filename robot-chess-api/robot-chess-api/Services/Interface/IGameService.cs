using robot_chess_api.DTOs;

namespace robot_chess_api.Services.Interface
{
    public interface IGameService
    {
        Task<IEnumerable<GameTypeDto>> GetAllGameTypesAsync();
        Task<StartGameResponseDto> StartGameAsync(StartGameRequestDto request, Guid? playerId);
        Task<StartGameResponseDto> ResumeGameAsync(ResumeGameRequestDto request);
        Task<GameDto?> GetGameByIdAsync(Guid id);
        Task<IEnumerable<GameDto>> GetPlayerGamesAsync(Guid playerId);
        Task<BoardSetupStatusDto> VerifyBoardSetupAsync(Guid gameId);
        
        // Game move methods
        Task<GameMoveDto> SaveMoveAsync(CreateGameMoveDto moveDto);
        Task<IEnumerable<GameMoveDto>> SaveMovesAsync(SaveMovesRequestDto request);
        Task<IEnumerable<GameMoveDto>> GetGameMovesAsync(Guid gameId);
        Task<IEnumerable<GameMoveDto>> GetGameMovesRangeAsync(GetMovesRequestDto request);
        
        // Update game result
        Task<UpdateGameResultResponseDto> UpdateGameResultAsync(UpdateGameResultRequestDto request);
        
        // End game and notify AI
        Task<EndGameResponseDto> EndGameAsync(Guid gameId, string reason);
        
        // Game replay
        Task<GameReplayDto?> GetGameReplayAsync(Guid gameId);
    }
}
