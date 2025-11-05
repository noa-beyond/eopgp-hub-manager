using Newtonsoft.Json;

namespace Domain.Entities
{
    public abstract class GeometryData
    {
        public abstract string Type { get; }
    }

    public class PolygonGeometry : GeometryData
    {
        [JsonProperty("type")]
        public override string Type => "Polygon";
        [JsonProperty("coordinates")]
        public List<List<List<double>>> Coordinates { get; set; }
    }

    public class MultiPolygonGeometry : GeometryData
    {
        [JsonProperty("type")]
        public override string Type => "MultiPolygon";
        [JsonProperty("coordinates")]
        public List<List<List<List<double>>>> Coordinates { get; set; }
    }
}