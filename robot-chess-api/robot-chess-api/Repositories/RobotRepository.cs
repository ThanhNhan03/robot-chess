using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using robot_chess_api.Data;
using robot_chess_api.Models;

namespace robot_chess_api.Repositories;

/// <summary>
/// Implementation của Robot Repository
/// </summary>
public class RobotRepository : IRobotRepository
{
    private readonly PostgresContext _context;

    public RobotRepository(PostgresContext context)
    {
        _context = context;
    }

    // Basic CRUD
    public async Task<IEnumerable<Robot>> GetAllAsync()
    {
        return await _context.Robots
            .Include(r => r.RobotConfig)
            .Include(r => r.CurrentGame)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Robot?> GetByIdAsync(Guid id)
    {
        return await _context.Robots
            .Include(r => r.RobotConfig)
            .Include(r => r.CurrentGame)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Robot?> GetByCodeAsync(string robotCode)
    {
        return await _context.Robots
            .Include(r => r.RobotConfig)
            .FirstOrDefaultAsync(r => r.RobotCode == robotCode);
    }

    public async Task<Robot> CreateAsync(Robot robot)
    {
        robot.Id = Guid.NewGuid();
        robot.CreatedAt = DateTime.UtcNow;
        robot.UpdatedAt = DateTime.UtcNow;
        
        _context.Robots.Add(robot);
        await _context.SaveChangesAsync();
        
        return robot;
    }

    public async Task<Robot> UpdateAsync(Robot robot)
    {
        robot.UpdatedAt = DateTime.UtcNow;
        
        _context.Robots.Update(robot);
        await _context.SaveChangesAsync();
        
        return robot;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var robot = await _context.Robots.FindAsync(id);
        if (robot == null) return false;

        _context.Robots.Remove(robot);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Robots.AnyAsync(r => r.Id == id);
    }

    // Config operations
    public async Task<RobotConfig?> GetConfigAsync(Guid robotId)
    {
        return await _context.Set<RobotConfig>()
            .FirstOrDefaultAsync(c => c.RobotId == robotId);
    }

    public async Task<RobotConfig> CreateOrUpdateConfigAsync(RobotConfig config)
    {
        var existing = await GetConfigAsync(config.RobotId);
        
        if (existing == null)
        {
            config.Id = Guid.NewGuid();
            config.CreatedAt = DateTime.UtcNow;
            config.UpdatedAt = DateTime.UtcNow;
            _context.Set<RobotConfig>().Add(config);
        }
        else
        {
            existing.Speed = config.Speed;
            existing.GripperForce = config.GripperForce;
            existing.GripperSpeed = config.GripperSpeed;
            existing.MaxSpeed = config.MaxSpeed;
            existing.EmergencyStop = config.EmergencyStop;
            existing.UpdatedBy = config.UpdatedBy;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.Set<RobotConfig>().Update(existing);
            config = existing;
        }
        
        await _context.SaveChangesAsync();
        return config;
    }

    // Command history operations
    public async Task<RobotCommandHistory> AddCommandHistoryAsync(RobotCommandHistory command)
    {
        command.Id = Guid.NewGuid();
        command.SentAt = DateTime.UtcNow;
        command.Status = "pending";
        
        _context.Set<RobotCommandHistory>().Add(command);
        await _context.SaveChangesAsync();
        
        return command;
    }

    public async Task<RobotCommandHistory> UpdateCommandHistoryAsync(RobotCommandHistory command)
    {
        _context.Set<RobotCommandHistory>().Update(command);
        await _context.SaveChangesAsync();
        
        return command;
    }

    public async Task<IEnumerable<RobotCommandHistory>> GetCommandHistoryAsync(Guid robotId, int limit = 50)
    {
        return await _context.Set<RobotCommandHistory>()
            .Where(c => c.RobotId == robotId)
            .OrderByDescending(c => c.SentAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<RobotCommandHistory?> GetCommandByIdAsync(Guid commandId)
    {
        return await _context.Set<RobotCommandHistory>()
            .FirstOrDefaultAsync(c => c.Id == commandId);
    }

    // Status operations
    public async Task<bool> UpdateOnlineStatusAsync(Guid robotId, bool isOnline)
    {
        var robot = await _context.Robots.FindAsync(robotId);
        if (robot == null) return false;

        robot.IsOnline = isOnline;
        robot.LastOnlineAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateRobotStatusAsync(Guid robotId, string status)
    {
        var robot = await _context.Robots.FindAsync(robotId);
        if (robot == null) return false;

        robot.Status = status;
        robot.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}
