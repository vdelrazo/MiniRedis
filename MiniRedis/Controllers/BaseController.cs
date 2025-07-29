using Microsoft.AspNetCore.Mvc;
using MiniRedis.Models;
using MiniRedis.Utils;

namespace MiniRedis.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected IActionResult Error(string message)
        {
            MetricsTracker.TrackError();
            return BadRequest(new ErrorResponse { Error = message });
        }
    }
}