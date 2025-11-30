namespace RobotChessServer.Services
{
    /// <summary>
    /// Interface cho TCP Service
    /// </summary>
    public interface ITcpService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync();
    }
}
