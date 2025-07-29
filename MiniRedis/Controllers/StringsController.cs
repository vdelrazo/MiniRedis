using Microsoft.AspNetCore.Mvc;
using MiniRedis.Services;
using MiniRedis.Utils;
using MiniRedis.Models.Requests;

namespace MiniRedis.Controllers
{
    [ApiController]
    [Route("strings")]
    public class StringsController : BaseController
    {
        private readonly IRedisService _redis;
        private readonly ILogger<StringsController> _logger;

        public StringsController(IRedisService redis, ILogger<StringsController> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        // POST /strings with { "key": "foo", "value": "bar", "ttlSeconds": 60 }
        [HttpPost("{key}")]
        public IActionResult Set(string key, [FromBody] SetRequest body)
        {
            if (string.IsNullOrWhiteSpace(key) || body == null || string.IsNullOrWhiteSpace(body.Value))
            {
                MetricsTracker.TrackError();
                _logger.LogWarning("POST /strings/{Key} failed: key or value missing", key);
                return Error("Key or value missing");
            }

            if (!ValidationHelper.IsValidToken(key) || !ValidationHelper.IsValidToken(body.Value))
            {
                MetricsTracker.TrackError();
                _logger.LogWarning("POST /strings/{Key} failed: invalid key or value. Value={Value}", key, body.Value);
                return Error("Invalid characters in key or value");
            }

            var result = body.TtlSeconds.HasValue
                ? _redis.Set(key, body.Value, body.TtlSeconds.Value)
                : _redis.Set(key, body.Value);

            MetricsTracker.Track("SET");
            _logger.LogInformation("POST /strings/{Key} succeeded", key);
            return Ok(new { result });
        }

        // GET /strings/{key}
        [HttpGet("{key}")]
        public IActionResult Get(string key)
        {
            if (!ValidationHelper.IsValidToken(key))
            {
                MetricsTracker.TrackError();
                _logger.LogWarning("GET /strings/{Key} failed: invalid key", key);
                return Error("Invalid key");
            }

            var result = _redis.Get(key);
            MetricsTracker.Track("GET");
            _logger.LogInformation("GET /strings/{Key} returned value", key);
            return Ok(new { result });
        }

        // DELETE /strings/{key}
        [HttpDelete("{key}")]
        public IActionResult Delete(string key)
        {
            if (!ValidationHelper.IsValidToken(key))
            {
                MetricsTracker.TrackError();
                _logger.LogWarning("DELETE /strings/{Key} failed: invalid key", key);
                return Error("Invalid key");
            }

            var result = _redis.Del(key);
            MetricsTracker.Track("DEL");
            _logger.LogInformation("DELETE /strings/{Key} removed", key);
            return Ok(new { result });
        }

        // POST /strings/{key}/increment
        [HttpPost("{key}/increment")]
        public IActionResult Incr(string key)
        {
            if (!ValidationHelper.IsValidToken(key))
            {
                MetricsTracker.TrackError();
                _logger.LogWarning("POST /strings/{Key}/increment failed: invalid key", key);
                return Error("Invalid key");
            }

            var result = _redis.Incr(key);
            MetricsTracker.Track("INCR");
            _logger.LogInformation("POST /strings/{Key}/increment incremented", key);
            return Ok(new { result });
        }
    }
}