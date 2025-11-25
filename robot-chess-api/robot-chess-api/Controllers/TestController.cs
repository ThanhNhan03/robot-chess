using Microsoft.AspNetCore.Mvc;

namespace robot_chess_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult PublicEndpoint()
    {
        return Ok(new { message = "This is a public endpoint" });
    }

    [HttpGet("protected")]
    [Helpers.Authorize]
    public IActionResult ProtectedEndpoint()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        return Ok(new 
        { 
            message = "This is a protected endpoint",
            userId = userId,
            email = email
        });
    }
}
