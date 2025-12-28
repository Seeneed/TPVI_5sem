using ASPA0011_1.Models;
using ASPA0011_1.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASPA0011_1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly IChannelService _channelService;
        private readonly ILogger<QueueController> _logger;

        public QueueController(IChannelService channelService, ILogger<QueueController> logger)
        {
            _channelService = channelService;
            _logger = logger;
            _logger.LogTrace("QueueController instance created.");
        }

        [HttpPost]
        public async Task<IActionResult> QueueOperation([FromBody] QueueRequest request)
        {
            _logger.LogDebug("Request received for POST /api/queue with command {command}", request.Command);
            var result = await _channelService.ProcessQueueRequest(request);
            if (result is ErrorResponse)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
    }
}