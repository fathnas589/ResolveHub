namespace ResolveHub.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using ResolveHub.DataAccess.Connection;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IDatabaseHealthCheck _databaseHealthCheck;

    public HealthController(IDatabaseHealthCheck databaseHealthCheck)
    {
        _databaseHealthCheck = databaseHealthCheck;
    }

    /// <summary>
    /// Checks database connectivity and retrieves server metadata.
    /// </summary>
    [HttpGet("db")]
    public async Task<IActionResult> CheckDatabase()
    {
        var health = await _databaseHealthCheck.GetDatabaseHealthAsync();

        if (!health.IsConnected)
        {
            return StatusCode(503, health);
        }

        return Ok(health);
    }
}
