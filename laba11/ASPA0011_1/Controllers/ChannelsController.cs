using ASPA0011_1.Models;
using ASPA0011_1.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASPA0011_1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChannelsController : ControllerBase
    {
        private readonly IChannelService _channelService;
        private readonly ILogger<ChannelsController> _logger;

        public ChannelsController(IChannelService channelService, ILogger<ChannelsController> logger)
        {
            _channelService = channelService;
            _logger = logger;
            _logger.LogTrace("ChannelsController instance created.");
        }

        [HttpGet]
        public IActionResult GetChannels()
        {
            _logger.LogDebug("Request received for GET /api/channels");
            var channels = _channelService.GetAllChannels();
            if (!channels.Any())
            {
                return NoContent();
            }
            return Ok(channels.Select(c => c.ToJson()));
        }

        [HttpGet("{guid}")]
        public IActionResult GetChannel(Guid guid)
        {
            _logger.LogDebug("Getting channel by id: {guid}", guid);
            var channel = _channelService.GetChannelById(guid);
            if (channel == null)
            {
                _logger.LogError("Channel with id {guid} not found.", guid);
                return NotFound();
            }
            return Ok(channel.ToJson());
        }

        [HttpPost]
        public IActionResult CreateChannel([FromBody] CreateChannelRequest request)
        {
            _logger.LogDebug("Request received for POST /api/channels");
            var channel = _channelService.CreateChannel(request);
            var status = channel.State == ChannelState.ACTIVE ? 201 : 204;
            return StatusCode(status, channel.ToJson());
        }

        [HttpPut]
        public IActionResult UpdateChannels([FromBody] UpdateChannelsRequest request)
        {
            _logger.LogDebug("Request received for PUT /api/channels");
            if (request.Id.HasValue)
            {
                var channel = _channelService.UpdateChannelById(request);
                if (channel == null)
                {
                    return NotFound();
                }
                return Ok(channel.ToJson());
            }
            else
            {
                var channels = _channelService.UpdateAllChannels(request);
                return Ok(channels.Select(c => c.ToJson()));
            }
        }

        [HttpDelete]
        public IActionResult DeleteChannels([FromBody] DeleteChannelsRequest request)
        {
            _logger.LogDebug("Request received for DELETE /api/channels");
            var deletedChannels = _channelService.DeleteChannels(request);
            if (deletedChannels.Any())
            {
                return Ok(deletedChannels.Select(c => c.ToJson()));
            }
            return NotFound();
        }
    }
}