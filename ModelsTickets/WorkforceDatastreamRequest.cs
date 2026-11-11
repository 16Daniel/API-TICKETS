using System.Text.Json.Serialization;

namespace TICKETSAPI.ModelsTickets
{
    public class WorkforceDatastreamRequest
    {
        [JsonPropertyName("datastream_id")]
        public int DatastreamId { get; set; }

        [JsonPropertyName("time")]
        public long Time { get; set; }

        [JsonPropertyName("stat")]
        public decimal Stat { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
    }
}
