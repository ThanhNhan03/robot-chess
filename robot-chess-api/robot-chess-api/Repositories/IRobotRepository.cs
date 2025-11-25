using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using robot_chess_api.Models;

namespace robot_chess_api.Repositories;

/// <summary>
/// Interface cho Robot Repository
/// </summary>
public interface IRobotRepository
{
    // Basic CRUD
    Task<IEnumerable<Robot>> GetAllAsync();
    Task<Robot?> GetByIdAsync(Guid id);
    Task<Robot?> GetByCodeAsync(string robotCode);
    Task<Robot> CreateAsync(Robot robot);
    Task<Robot> UpdateAsync(Robot robot);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);

    // Config operations
    Task<RobotConfig?> GetConfigAsync(Guid robotId);
    Task<RobotConfig> CreateOrUpdateConfigAsync(RobotConfig config);

    // Monitoring operations
    Task<RobotMonitoring> AddMonitoringAsync(RobotMonitoring monitoring);
    Task<IEnumerable<RobotMonitoring>> GetMonitoringHistoryAsync(Guid robotId, int limit = 100);
    Task<RobotMonitoring?> GetLatestMonitoringAsync(Guid robotId);

    // Command history operations
    Task<RobotCommandHistory> AddCommandHistoryAsync(RobotCommandHistory command);
    Task<RobotCommandHistory> UpdateCommandHistoryAsync(RobotCommandHistory command);
    Task<IEnumerable<RobotCommandHistory>> GetCommandHistoryAsync(Guid robotId, int limit = 50);
    Task<RobotCommandHistory?> GetCommandByIdAsync(Guid commandId);

    // Status operations
    Task<bool> UpdateOnlineStatusAsync(Guid robotId, bool isOnline);
    Task<bool> UpdateRobotStatusAsync(Guid robotId, string status);
}
