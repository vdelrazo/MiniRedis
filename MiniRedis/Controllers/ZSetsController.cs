using Microsoft.AspNetCore.Mvc;
using MiniRedis.Services;
using MiniRedis.Models.Requests;
using MiniRedis.Utils;
using System.Text.Json;

namespace MiniRedis.Controllers
{
    [ApiController]
    [Route("zsets")]
    public class ZSetsController : BaseController
    {
        private readonly IRedisService _redis;
        private readonly ILogger<ZSetsController> _logger;

        public ZSetsController(IRedisService redis, ILogger<ZSetsController> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        // POST /zsets/{key}
        [HttpPost("{key}")]
        public IActionResult ZAdd(string key, [FromBody] JsonElement payload)
        {
            if (!ValidationHelper.IsValidToken(key))
            {
                MetricsTracker.TrackError();
                _logger.LogWarning("ZADD failed: Invalid key {Key}", key);
                return Error("Invalid key.");
            }

            var members = new List<(double score, string member)>();

            // Arreglo de miembros
            if (payload.TryGetProperty("members", out var memberArray) && memberArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in memberArray.EnumerateArray())
                {
                    if (!item.TryGetProperty("member", out var memberProp) ||
                        !item.TryGetProperty("score", out var scoreProp) ||
                        memberProp.ValueKind != JsonValueKind.String ||
                        scoreProp.ValueKind != JsonValueKind.Number)
                    {
                        return Error("Invalid member entry.");
                    }

                    members.Add((scoreProp.GetDouble(), memberProp.GetString()!));
                }
            }
            // Un solo miembro
            else if (payload.TryGetProperty("member", out var singleMemberProp) &&
                    payload.TryGetProperty("score", out var singleScoreProp) &&
                    singleMemberProp.ValueKind == JsonValueKind.String &&
                    singleScoreProp.ValueKind == JsonValueKind.Number)
            {
                members.Add((singleScoreProp.GetDouble(), singleMemberProp.GetString()!));
            }
            else
            {
                return Error("Invalid request body format.");
            }

            // ✅ Aquí ya llamas correctamente al único método válido
            var addedCount = _redis.ZAdd(key, members);

            MetricsTracker.Track("ZADD");
            _logger.LogInformation("ZADD success: Key={Key}, Added={Added}", key, addedCount);
            return Ok(new { added = addedCount });
        }

        // GET /zsets/{key}/count
        [HttpGet("{key}/count")]
        public IActionResult ZCard(string key)
        {
            if (!ValidationHelper.IsValidToken(key))
            {
                MetricsTracker.TrackError();
                _logger.LogWarning("ZCARD failed: Invalid key {Key}", key);
                return Error("Invalid key.");
            }

            var result = _redis.ZCard(key);
            MetricsTracker.Track("ZCARD");
            _logger.LogInformation("ZCARD success: Key={Key}", key);
            return Ok(new { result });
        }

        // GET /zsets/{key}/rank/{member}
        [HttpGet("{key}/rank/{member}")]
        public IActionResult ZRank(string key, string member)
        {
            if (!ValidationHelper.IsValidToken(key) || !ValidationHelper.IsValidToken(member))
            {
                MetricsTracker.TrackError();
                _logger.LogWarning("ZRANK failed: Invalid key or member. Key={Key}, Member={Member}", key, member);
                return Error("Invalid key or member.");
            }

            var result = _redis.ZRank(key, member);
            MetricsTracker.Track("ZRANK");
            _logger.LogInformation("ZRANK success: Key={Key}, Member={Member}", key, member);
            return Ok(new { result });
        }

        // GET /zsets/{key}/range?start=0&stop=1
        [HttpGet("{key}/range")]
        public IActionResult ZRange(string key, [FromQuery] int start, [FromQuery] int stop)
        {
            if (!ValidationHelper.IsValidToken(key))
            {
                MetricsTracker.TrackError();
                _logger.LogWarning("ZRANGE failed: Invalid key {Key}", key);
                return Error("Invalid key.");
            }

            var result = _redis.ZRange(key, start, stop);
            MetricsTracker.Track("ZRANGE");
            _logger.LogInformation("ZRANGE success: Key={Key}, Start={Start}, Stop={Stop}", key, start, stop);
            return Ok(new { result });
        }
    }
}