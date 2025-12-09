using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using robot_chess_api.DTOs;
using robot_chess_api.Models;
using robot_chess_api.Repositories;
using robot_chess_api.Services.Interface;

namespace robot_chess_api.Services.Implement;

/// <summary>
/// Implementation của Robot Service
/// </summary>
public class RobotService : IRobotService
{
    private readonly IRobotRepository _robotRepository;

    public RobotService(IRobotRepository robotRepository)
    {
        _robotRepository = robotRepository;
    }

    // Basic CRUD
    public async Task<IEnumerable<RobotDto>> GetAllRobotsAsync()
    {
        var robots = await _robotRepository.GetAllAsync();
        var robotDtos = new List<RobotDto>();

        foreach (var robot in robots)
        {
            var dto = MapToDto(robot);
            robotDtos.Add(dto);
        }

        return robotDtos;
    }

    public async Task<RobotDto?> GetRobotByIdAsync(Guid id)
    {
        var robot = await _robotRepository.GetByIdAsync(id);
        if (robot == null) return null;

        var dto = MapToDto(robot);
        
        // Get config
        var config = await _robotRepository.GetConfigAsync(id);
        if (config != null)
        {
            dto.Config = MapConfigToDto(config);
        }

        return dto;
    }

    public async Task<RobotDto?> GetRobotByCodeAsync(string robotCode)
    {
        var robot = await _robotRepository.GetByCodeAsync(robotCode);
        if (robot == null) return null;

        return MapToDto(robot);
    }

    public async Task<RobotDto> CreateRobotAsync(CreateRobotDto dto)
    {
        var robot = new Robot
        {
            RobotCode = dto.RobotCode,
            Name = dto.Name,
            Location = dto.Location,
            IpAddress = dto.IpAddress,
            IsOnline = false,
            Status = "idle"
        };

        var created = await _robotRepository.CreateAsync(robot);

        // Create default config
        var config = new RobotConfig
        {
            RobotId = created.Id,
            Speed = 50,
            GripperForce = 50,
            GripperSpeed = 50,
            MaxSpeed = 100,
            EmergencyStop = false
        };
        await _robotRepository.CreateOrUpdateConfigAsync(config);

        return MapToDto(created);
    }

    public async Task<RobotDto> UpdateRobotAsync(Guid id, UpdateRobotDto dto)
    {
        var robot = await _robotRepository.GetByIdAsync(id);
        if (robot == null)
            throw new Exception($"Robot with ID {id} not found");

        if (dto.Name != null) robot.Name = dto.Name;
        if (dto.Location != null) robot.Location = dto.Location;
        if (dto.IpAddress != null) robot.IpAddress = dto.IpAddress;
        if (dto.Status != null) robot.Status = dto.Status;

        var updated = await _robotRepository.UpdateAsync(robot);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteRobotAsync(Guid id)
    {
        return await _robotRepository.DeleteAsync(id);
    }

    // Config operations
    public async Task<RobotConfigDto?> GetRobotConfigAsync(Guid robotId)
    {
        var config = await _robotRepository.GetConfigAsync(robotId);
        return config != null ? MapConfigToDto(config) : null;
    }

    public async Task<RobotConfigDto> UpdateRobotConfigAsync(Guid robotId, UpdateRobotConfigDto dto, Guid? updatedBy = null)
    {
        var config = await _robotRepository.GetConfigAsync(robotId);
        if (config == null)
        {
            config = new RobotConfig { RobotId = robotId };
        }

        if (dto.Speed.HasValue) config.Speed = dto.Speed;
        if (dto.GripperForce.HasValue) config.GripperForce = dto.GripperForce;
        if (dto.GripperSpeed.HasValue) config.GripperSpeed = dto.GripperSpeed;
        if (dto.MaxSpeed.HasValue) config.MaxSpeed = dto.MaxSpeed;
        config.UpdatedBy = updatedBy;

        var updated = await _robotRepository.CreateOrUpdateConfigAsync(config);
        return MapConfigToDto(updated);
    }

    public async Task<bool> UpdateSpeedAsync(Guid robotId, int speed, Guid? updatedBy = null)
    {
        if (speed < 10 || speed > 100)
            throw new ArgumentException("Speed must be between 10 and 100");

        var dto = new UpdateRobotConfigDto { Speed = speed };
        await UpdateRobotConfigAsync(robotId, dto, updatedBy);
        
        return true;
    }

    // Status operations
    public async Task<RobotStatusDto?> GetRobotStatusAsync(Guid robotId)
    {
        var robot = await _robotRepository.GetByIdAsync(robotId);
        if (robot == null) return null;

        return new RobotStatusDto
        {
            Id = robot.Id,
            RobotCode = robot.RobotCode,
            Name = robot.Name,
            IsOnline = robot.IsOnline,
            LastOnlineAt = robot.LastOnlineAt,
            Status = robot.Status
        };
    }

    public async Task<IEnumerable<RobotStatusDto>> GetAllRobotsStatusAsync()
    {
        var robots = await _robotRepository.GetAllAsync();
        var statusList = new List<RobotStatusDto>();

        foreach (var robot in robots)
        {
            var status = await GetRobotStatusAsync(robot.Id);
            if (status != null)
            {
                statusList.Add(status);
            }
        }

        return statusList;
    }

    // Command operations
    public async Task<Guid> SendHomeCommandAsync(Guid robotId, Guid? executedBy = null)
    {
        var command = new RobotCommandHistory
        {
            RobotId = robotId,
            CommandType = "home",
            ExecutedBy = executedBy
        };

        var created = await _robotRepository.AddCommandHistoryAsync(command);
        return created.Id;
    }

    public async Task<Guid> SendCalibrateCommandAsync(Guid robotId, string? calibrationType = null, Guid? executedBy = null)
    {
        var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { type = calibrationType ?? "full" }));
        
        var command = new RobotCommandHistory
        {
            RobotId = robotId,
            CommandType = "calibrate",
            CommandPayload = payload,
            ExecutedBy = executedBy
        };

        var created = await _robotRepository.AddCommandHistoryAsync(command);
        return created.Id;
    }

    public async Task<Guid> SendTestGripperCommandAsync(Guid robotId, int? position = null, Guid? executedBy = null)
    {
        var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { position = position ?? 50 }));
        
        var command = new RobotCommandHistory
        {
            RobotId = robotId,
            CommandType = "test_gripper",
            CommandPayload = payload,
            ExecutedBy = executedBy
        };

        var created = await _robotRepository.AddCommandHistoryAsync(command);
        return created.Id;
    }

    public async Task<Guid> SendEmergencyStopAsync(Guid robotId, Guid? executedBy = null)
    {
        var command = new RobotCommandHistory
        {
            RobotId = robotId,
            CommandType = "emergency_stop",
            ExecutedBy = executedBy
        };

        var created = await _robotRepository.AddCommandHistoryAsync(command);
        
        // Also update config
        var config = await _robotRepository.GetConfigAsync(robotId);
        if (config != null)
        {
            config.EmergencyStop = true;
            await _robotRepository.CreateOrUpdateConfigAsync(config);
        }

        return created.Id;
    }

    public async Task<Guid> SendSetSpeedCommandAsync(Guid robotId, int speed, Guid? executedBy = null)
    {
        var payload = JsonDocument.Parse(JsonSerializer.Serialize(new { speed }));
        
        var command = new RobotCommandHistory
        {
            RobotId = robotId,
            CommandType = "set_speed",
            CommandPayload = payload,
            ExecutedBy = executedBy
        };

        var created = await _robotRepository.AddCommandHistoryAsync(command);
        
        // Also update config
        await UpdateSpeedAsync(robotId, speed, executedBy);

        return created.Id;
    }

    // Command history
    public async Task<IEnumerable<RobotCommandHistoryDto>> GetCommandHistoryAsync(Guid robotId, int limit = 50)
    {
        var history = await _robotRepository.GetCommandHistoryAsync(robotId, limit);
        return history.Select(h => new RobotCommandHistoryDto
        {
            Id = h.Id,
            RobotId = h.RobotId,
            CommandType = h.CommandType,
            Payload = h.CommandPayload,
            Status = h.Status ?? "",
            ErrorMessage = h.ErrorMessage,
            SentAt = h.SentAt ?? DateTime.UtcNow,
            StartedAt = h.StartedAt,
            CompletedAt = h.CompletedAt,
            ExecutionTimeMs = h.ExecutionTimeMs,
            ExecutedBy = h.ExecutedBy
        });
    }

    public async Task<bool> UpdateCommandStatusAsync(Guid commandId, string status, string? errorMessage = null, int? executionTimeMs = null)
    {
        var command = await _robotRepository.GetCommandByIdAsync(commandId);
        if (command == null) return false;

        command.Status = status;
        
        if (status == "executing" && command.StartedAt == null)
        {
            command.StartedAt = DateTime.UtcNow;
        }
        
        if (status == "completed" || status == "failed")
        {
            command.CompletedAt = DateTime.UtcNow;
            if (command.StartedAt.HasValue)
            {
                command.ExecutionTimeMs = (int)(command.CompletedAt.Value - command.StartedAt.Value).TotalMilliseconds;
            }
        }
        
        if (errorMessage != null)
        {
            command.ErrorMessage = errorMessage;
        }
        
        if (executionTimeMs.HasValue)
        {
            command.ExecutionTimeMs = executionTimeMs;
        }

        await _robotRepository.UpdateCommandHistoryAsync(command);
        return true;
    }

    // Helper methods
    private RobotDto MapToDto(Robot robot)
    {
        return new RobotDto
        {
            Id = robot.Id,
            RobotCode = robot.RobotCode,
            Name = robot.Name,
            Location = robot.Location,
            IpAddress = robot.IpAddress,
            IsOnline = robot.IsOnline,
            LastOnlineAt = robot.LastOnlineAt,
            Status = robot.Status,
            CurrentGameId = robot.CurrentGameId,
            MoveSpeedMs = robot.MoveSpeedMs,
            CreatedAt = robot.CreatedAt,
            UpdatedAt = robot.UpdatedAt
        };
    }

    private RobotConfigDto MapConfigToDto(RobotConfig config)
    {
        return new RobotConfigDto
        {
            Id = config.Id,
            RobotId = config.RobotId,
            Speed = config.Speed,
            GripperForce = config.GripperForce,
            GripperSpeed = config.GripperSpeed,
            MaxSpeed = config.MaxSpeed,
            EmergencyStop = config.EmergencyStop,
            UpdatedAt = config.UpdatedAt
        };
    }
}
