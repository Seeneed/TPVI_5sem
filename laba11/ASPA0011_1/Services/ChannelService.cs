using ASPA0011_1.Models;
using System.Collections.Concurrent;

namespace ASPA0011_1.Services
{
    public class ChannelService : IChannelService
    {
        private readonly ConcurrentDictionary<Guid, ChannelInfo> _channels = new();
        private readonly ILogger<ChannelService> _logger;
        private readonly int _waitEnqueueSeconds;

        public ChannelService(ILogger<ChannelService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _waitEnqueueSeconds = configuration.GetValue<int>("WaitEnqueue", 30);
            _logger.LogInformation(1, "ChannelService initialized. WaitEnqueue is set to {WaitEnqueueSeconds} seconds.", _waitEnqueueSeconds);
        }

        public ChannelInfo CreateChannel(CreateChannelRequest request)
        {
            var channelInfo = new ChannelInfo(request.Name, request.State, request.Description);
            _channels.TryAdd(channelInfo.Id, channelInfo);
            _logger.LogInformation(2, "Channel created. ID: {ChannelId}, Name: {ChannelName}, State: {ChannelState}", channelInfo.Id, channelInfo.Name, channelInfo.State);
            return channelInfo;
        }

        public IEnumerable<ChannelInfo> DeleteChannels(DeleteChannelsRequest request)
        {
            var channelsToDelete = _channels.Values.AsEnumerable();

            if (!string.IsNullOrEmpty(request.State) && Enum.TryParse<ChannelState>(request.State, true, out var state))
            {
                channelsToDelete = channelsToDelete.Where(c => c.State == state);
            }

            var deletedChannels = new List<ChannelInfo>();
            foreach (var channel in channelsToDelete.ToList())
            {
                if (_channels.TryRemove(channel.Id, out var removedChannel))
                {
                    deletedChannels.Add(removedChannel);
                    _logger.LogInformation(3, "Channel deleted. ID: {ChannelId}", removedChannel.Id);
                }
            }
            return deletedChannels;
        }

        public IEnumerable<ChannelInfo> GetAllChannels()
        {
            return _channels.Values;
        }

        public ChannelInfo? GetChannelById(Guid id)
        {
            _channels.TryGetValue(id, out var channel);
            return channel;
        }

        public IEnumerable<ChannelInfo> UpdateAllChannels(UpdateChannelsRequest request)
        {
            var updatedChannels = new List<ChannelInfo>();
            foreach (var channel in _channels.Values)
            {
                UpdateChannelState(channel, request.Command);
                updatedChannels.Add(channel);
            }
            return updatedChannels;
        }

        public ChannelInfo? UpdateChannelById(UpdateChannelsRequest request)
        {
            if (request.Id.HasValue && _channels.TryGetValue(request.Id.Value, out var channel))
            {
                UpdateChannelState(channel, request.Command);
                return channel;
            }
            return null;
        }

        private void UpdateChannelState(ChannelInfo channel, string command)
        {
            var newState = command.ToLower() == "open" ? ChannelState.ACTIVE : ChannelState.CLOSED;

            if (channel.State == newState)
            {
                _logger.LogWarning(4, "Channel {ChannelId} is already in {State} state.", channel.Id, newState);
            }
            else
            {
                channel.State = newState;
                _logger.LogInformation(5, "Channel {ChannelId} state changed to {State}.", channel.Id, newState);
            }
        }

        public async Task<object> ProcessQueueRequest(QueueRequest request)
        {
            if (!_channels.TryGetValue(request.Id, out var channelInfo) || channelInfo.State == ChannelState.CLOSED)
            {
                _logger.LogError(6, "Channel {ChannelId} not found or is closed.", request.Id);
                return new ErrorResponse { Id = request.Id, Error = "Channel not found or is closed." };
            }

            switch (request.Command.ToLower())
            {
                case "enqueue":
                    if (string.IsNullOrEmpty(request.Data))
                    {
                        return new ErrorResponse { Id = request.Id, Error = "Data is required for enqueue." };
                    }
                    try
                    {
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_waitEnqueueSeconds));
                        await channelInfo.Channel.Writer.WriteAsync(request.Data, cts.Token);
                        _logger.LogDebug(7, "Enqueued data to channel {ChannelId}. Data: {Data}", request.Id, request.Data);
                        return new QueueResponse { Id = request.Id, Data = request.Data };
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning(8, "Enqueue operation timed out for channel {ChannelId} after {WaitEnqueueSeconds} seconds.", request.Id, _waitEnqueueSeconds);
                        return new ErrorResponse { Id = request.Id, Error = $"Enqueue operation timed out after {_waitEnqueueSeconds} seconds." };
                    }

                case "dequeue":
                    if (channelInfo.Channel.Reader.TryRead(out var item))
                    {
                        _logger.LogDebug(9, "Dequeued data from channel {ChannelId}. Data: {Data}", request.Id, item);
                        return new QueueResponse { Id = request.Id, Data = item };
                    }
                    return new ErrorResponse { Id = request.Id, Error = "Queue is empty." };

                case "peek":
                    if (channelInfo.Channel.Reader.TryPeek(out var peekedItem))
                    {
                        _logger.LogDebug(10, "Peeked data from channel {ChannelId}. Data: {Data}", request.Id, peekedItem);
                        return new QueueResponse { Id = request.Id, Data = peekedItem };
                    }
                    return new ErrorResponse { Id = request.Id, Error = "Queue is empty." };

                default:
                    _logger.LogError(11, "Invalid command '{Command}' for queue operation on channel {ChannelId}.", request.Command, request.Id);
                    return new ErrorResponse { Id = request.Id, Error = "Invalid command." };
            }
        }
    }
}