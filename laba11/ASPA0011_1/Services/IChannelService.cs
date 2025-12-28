using ASPA0011_1.Models;

namespace ASPA0011_1.Services
{
    public interface IChannelService
    {
        IEnumerable<ChannelInfo> GetAllChannels();
        ChannelInfo? GetChannelById(Guid id);
        ChannelInfo CreateChannel(CreateChannelRequest request);
        IEnumerable<ChannelInfo> UpdateAllChannels(UpdateChannelsRequest request);
        ChannelInfo? UpdateChannelById(UpdateChannelsRequest request);
        IEnumerable<ChannelInfo> DeleteChannels(DeleteChannelsRequest request);
        Task<object> ProcessQueueRequest(QueueRequest request);
    }
}