using Newtonsoft.Json;

namespace Eventos.Models
{
    public class EventItem
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("zone")]
        public string Zone { get; set; }

        [JsonProperty("startTime")]
        public string StartTime { get; set; }

        [JsonProperty("endTime")]
        public string EndTime { get; set; }

        [JsonProperty("URLFile")]
        public string URLFile { get; set; }
    }

    public class Data
    {
        [JsonProperty("events")]
        public List<EventItem> Events { get; set; }
    }
}
