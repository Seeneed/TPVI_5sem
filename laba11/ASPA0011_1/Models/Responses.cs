namespace ASPA0011_1.Models
{
    public class QueueResponse
    {
        public Guid Id { get; set; }
        public string? Data { get; set; }
    }

    public class ErrorResponse
    {
        public Guid? Id { get; set; }
        public string Error { get; set; }
    }
}