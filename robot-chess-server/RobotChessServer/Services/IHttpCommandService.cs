namespace RobotChessServer.Services
{
    /// <summary>
    /// Interface for 
    /// Command Service
    /// </summary>
    public interface IHttpCommandService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync();
    }
}
