namespace Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public int GroupId { get; set; }
        public Guid OriginId { get; set; }
        public string ProductName { get; set; } = null!;
        public DateTime SensingDate { get; set; }
        public string Path { get; set; }
        public GeometryData Geometry { get; set; }
        public int ProviderId { get; set; }
        public Guid OrderId { get; set; }
    }
}