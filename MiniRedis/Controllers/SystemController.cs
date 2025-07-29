using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MiniRedis.Stores;
using MiniRedis.Utils;

namespace MiniRedis.Controllers
{
    [ApiController]
    [Route("")]
    public class SystemController : ControllerBase
    {
        private readonly InMemoryDataStore _store;
        private readonly ILogger<SystemController> _logger;

        public SystemController(InMemoryDataStore store, ILogger<SystemController> logger)
        {
            _store = store;
            _logger = logger;
        }

        // GET /dbsize
        [HttpGet("dbsize")]
        public IActionResult DbSize()
        {
            _logger.LogInformation("GET /dbsize called");
            MetricsTracker.Track("GET");
            var result = _store.DbSize();
            return Ok(new { result });
        }

        // GET /health
        [HttpGet("health")]
        public IActionResult Health([FromQuery] bool skipMetrics = false)
        {
            var uptime = DateTime.UtcNow - AppStatus.StartTime;

            _logger.LogInformation("GET /health called");

            return Ok(new
            {
                status = "healthy",
                uptime = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s",
                startedAtUtc = AppStatus.StartTime.ToString("o"),
                version = "1.0.0"
            });
        }

        // GET /metrics
        [HttpGet("metrics")]
        public IActionResult Metrics()
        {
            _logger.LogInformation("GET /metrics called");

            var metrics = new
            {
                totalCommands = MetricsTracker.TotalCommands,
                commands = MetricsTracker.CommandCounts,
                errors = MetricsTracker.Errors,
                keysInStore = _store.DbSize()
            };

            return Ok(metrics);
        }
    }
}
