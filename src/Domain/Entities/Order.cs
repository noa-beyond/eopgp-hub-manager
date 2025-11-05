namespace Domain.Entities
{
    public class Order
    {
        public required Guid Id { get; set; }
        public int StatusId { get; set; }
        public int PipelineId { get; set; }
        public string RequestDetails { get; set; }
        //public DateTime CreatedAt { get; set; }
    }
}
