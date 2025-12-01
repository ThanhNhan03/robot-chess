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
    }
}
