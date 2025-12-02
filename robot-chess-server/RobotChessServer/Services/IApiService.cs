namespace RobotChessServer.Services
{
    /// <summary>
    /// Interface for API communication service
    /// </summary>
    public interface IApiService
    {
        Task<bool> UpdateRobotStatusAsync(string robotId, bool isOnline, string status);
        Task<RobotConfigResponse?> GetRobotConfigAsync(string robotId);
        Task<bool> UpdateCommandStatusAsync(Guid commandId, string status, string? errorMessage = null);
    }

    public class RobotConfigResponse
    {
        public int? Speed { get; set; }
        public int? GripperForce { get; set; }
        public int? GripperSpeed { get; set; }
        public int? MaxSpeed { get; set; }
    }
}
