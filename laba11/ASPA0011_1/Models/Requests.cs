using System.ComponentModel.DataAnnotations;

namespace ASPA0011_1.Models
{
    public class CreateChannelRequest
    {
        public string Command { get; set; } = "new";
        [Required]
        public string Name { get; set; } = string.Empty;
        public ChannelState State { get; set; } = ChannelState.ACTIVE;
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateChannelsRequest
    {
        [Required]
        public string Command { get; set; } // "close", "open"
        public Guid? Id { get; set; }
        public string? Reason { get; set; }
        public ChannelState? State { get; set; }
    }

    public class DeleteChannelsRequest
    {
        [Required]
        public string Command { get; set; } = "del";
        public string? State { get; set; }
    }

    public class QueueRequest
    {
        [Required]
        public string Command { get; set; } // "enqueue", "dequeue", "peek"
        [Required]
        public Guid Id { get; set; }
        public string? Data { get; set; }
    }
}