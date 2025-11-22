using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using robot_chess_api.DTOs;

namespace robot_chess_api.Services.Interface;

/// <summary>
/// Interface cho Robot Service
/// </summary>
public interface IRobotService
{
    // Basic CRUD
    Task<IEnumerable<RobotDto>> GetAllRobotsAsync();
    Task<RobotDto?> GetRobotByIdAsync(Guid id);
    Task<RobotDto?> GetRobotByCodeAsync(string robotCode);
    Task<RobotDto> CreateRobotAsync(CreateRobotDto dto);
    Task<RobotDto> UpdateRobotAsync(Guid id, UpdateRobotDto dto);
    Task<bool> DeleteRobotAsync(Guid id);

    // Config operations
    Task<RobotConfigDto?> GetRobotConfigAsync(Guid robotId);
    Task<RobotConfigDto> UpdateRobotConfigAsync(Guid robotId, UpdateRobotConfigDto dto, Guid? updatedBy = null);
    Task<bool> UpdateSpeedAsync(Guid robotId, int speed, Guid? updatedBy = null);

    // Status operations
    Task<RobotStatusDto?> GetRobotStatusAsync(Guid robotId);
    Task<IEnumerable<RobotStatusDto>> GetAllRobotsStatusAsync();

    // Monitoring operations
    Task<IEnumerable<RobotMonitoringDto>> GetMonitoringHistoryAsync(Guid robotId, int limit = 100);
    Task<RobotMonitoringDto?> GetLatestMonitoringAsync(Guid robotId);

    // Command operations
    Task<Guid> SendHomeCommandAsync(Guid robotId, Guid? executedBy = null);
    Task<Guid> SendCalibrateCommandAsync(Guid robotId, string? calibrationType = null, Guid? executedBy = null);
    Task<Guid> SendTestGripperCommandAsync(Guid robotId, int? position = null, Guid? executedBy = null);
    Task<Guid> SendEmergencyStopAsync(Guid robotId, Guid? executedBy = null);
    Task<Guid> SendSetSpeedCommandAsync(Guid robotId, int speed, Guid? executedBy = null);

    // Command history
    Task<IEnumerable<RobotCommandHistoryDto>> GetCommandHistoryAsync(Guid robotId, int limit = 50);
    Task<bool> UpdateCommandStatusAsync(Guid commandId, string status, string? errorMessage = null, int? executionTimeMs = null);
}
