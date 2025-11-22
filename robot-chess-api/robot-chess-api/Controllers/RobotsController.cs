using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using robot_chess_api.DTOs;
using robot_chess_api.Services.Interface;
using System.Security.Claims;

namespace robot_chess_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize] // Tạm thời comment để dễ test, sau này sẽ uncomment
    public class RobotsController : ControllerBase
    {
        private readonly IRobotService _robotService;
        private readonly ILogger<RobotsController> _logger;

        public RobotsController(IRobotService robotService, ILogger<RobotsController> logger)
        {
            _robotService = robotService;
            _logger = logger;
        }

        // GET: api/robots
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RobotDto>>> GetAllRobots()
        {
            try
            {
                var robots = await _robotService.GetAllRobotsAsync();
                return Ok(robots);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all robots");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/robots/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<RobotDto>> GetRobotById(Guid id)
        {
            try
            {
                var robot = await _robotService.GetRobotByIdAsync(id);
                if (robot == null)
                {
                    return NotFound($"Robot with ID {id} not found");
                }
                return Ok(robot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting robot {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/robots/{id}/config
        [HttpGet("{id}/config")]
        public async Task<ActionResult<RobotConfigDto>> GetRobotConfig(Guid id)
        {
            try
            {
                var config = await _robotService.GetRobotConfigAsync(id);
                if (config == null)
                {
                    return NotFound($"Config for robot {id} not found");
                }
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting config for robot {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/robots/{id}/config
        [HttpPut("{id}/config")]
        public async Task<ActionResult<RobotConfigDto>> UpdateRobotConfig(Guid id, [FromBody] UpdateRobotConfigDto dto)
        {
            try
            {
                // Lấy User ID từ token nếu có
                Guid? userId = null;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid parsedId))
                {
                    userId = parsedId;
                }

                var updatedConfig = await _robotService.UpdateRobotConfigAsync(id, dto, userId);
                return Ok(updatedConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating config for robot {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/robots/{id}/commands/speed
        [HttpPost("{id}/commands/speed")]
        public async Task<ActionResult<Guid>> SetRobotSpeed(Guid id, [FromBody] int speed)
        {
            try
            {
                if (speed < 10 || speed > 100)
                {
                    return BadRequest("Speed must be between 10 and 100");
                }

                // Lấy User ID từ token nếu có
                Guid? userId = null;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out Guid parsedId))
                {
                    userId = parsedId;
                }

                var commandId = await _robotService.SendSetSpeedCommandAsync(id, speed, userId);
                return Ok(new { CommandId = commandId, Message = "Set speed command sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending set speed command to robot {id}");
                return StatusCode(500, "Internal server error");
            }
        }
        
        // POST: api/robots
        [HttpPost]
        public async Task<ActionResult<RobotDto>> CreateRobot([FromBody] CreateRobotDto dto)
        {
            try
            {
                var robot = await _robotService.CreateRobotAsync(dto);
                return CreatedAtAction(nameof(GetRobotById), new { id = robot.Id }, robot);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating robot");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
