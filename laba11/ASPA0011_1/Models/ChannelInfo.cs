using System.Threading.Channels;

namespace ASPA0011_1.Models
{
    public class ChannelInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ChannelState State { get; set; }
        public string Description { get; set; } = string.Empty;
        public Channel<string> Channel { get; set; }

        public ChannelInfo(string name, ChannelState state, string description)
        {
            Id = Guid.NewGuid();
            Name = name;
            State = state;
            Description = description;
            Channel = System.Threading.Channels.Channel.CreateUnbounded<string>();
        }

        public object ToJson()
        {
            return new { id = Id, name = Name, state = State.ToString(), description = Description };
        }
    }
}