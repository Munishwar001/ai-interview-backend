using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [ApiController]
    [Route("api/interview")]
    [Authorize]
    public class InterviewController(IConfiguration configuration) : BaseController
    {
        /// <summary>
        /// Returns ICE server config (STUN + TURN) for WebRTC peer connections.
        /// Credentials are kept server-side so they never ship in the frontend bundle.
        /// </summary>
        [HttpGet("ice-servers")]
        public IActionResult GetIceServers()
        {
            var turnUrl      = configuration["Turn:Url"];
            var turnUsername = configuration["Turn:Username"];
            var turnPassword = configuration["Turn:Password"];

            var servers = new List<object>
            {
                new { urls = new[] { "stun:stun.l.google.com:19302", "stun:stun1.l.google.com:19302" } }
            };

            // Only add TURN if configured
            if (!string.IsNullOrWhiteSpace(turnUrl) &&
                !string.IsNullOrWhiteSpace(turnUsername) &&
                !string.IsNullOrWhiteSpace(turnPassword))
            {
                servers.Add(new
                {
                    urls       = new[] { turnUrl },
                    username   = turnUsername,
                    credential = turnPassword
                });
            }

            return Ok(servers);
        }
    }
}
