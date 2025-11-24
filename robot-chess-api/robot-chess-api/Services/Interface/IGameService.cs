using robot_chess_api.DTOs;

namespace robot_chess_api.Services.Interface
{
    public interface IGameService
    {
        Task<GameDto> StartGameAsync(StartGameRequest request, Guid? userId);
        Task<bool> StopGameAsync(Guid gameId);
        Task<GameDto?> GetGameAsync(Guid gameId);
    }
}
