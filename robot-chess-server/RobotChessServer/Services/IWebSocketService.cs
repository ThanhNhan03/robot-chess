namespace RobotChessServer.Services
{
    /// <summary>
    /// Interface cho WebSocket Service
    /// </summary>
    public interface IWebSocketService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync();
    }
}
