using Newtonsoft.Json;

namespace Eventos.Models
{
    public class EventItem : Bindableitem
    {
        private string title;
        private string description;

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("title")]
        public string Title
        {
            get => this.title;
            set
            {
                this.title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        [JsonProperty("description")]
        public string Description
        {
            get => this.description;
            set
            {
                this.description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        [JsonProperty("date")]
        public DateTime Date { get; set; }
        [JsonProperty("zone")]
        public string Zone { get; set; }
        
        [JsonProperty("zoneId")]
        public string ZoneId { get; set; }



        [JsonProperty("startTime")]
        public string StartTime { get; set; }

        [JsonProperty("endTime")]
        public string EndTime { get; set; }

        public string URLFile { get; set; }

        [JsonProperty("rowKey")]
        public string RowKey { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty("count")]
        public string Count { get; set; }

        [JsonProperty("eventURL")]
        public string EventURl { get; set; }

        [JsonProperty("isDelete")]
        public bool IsDelete { get; set; }

        [JsonProperty("clickedInfo")]
        public List<EventClickedInfo> ClickedInfo { get; set; }

        override public string ToString()
        {
            return Title;
        }
    }

    public class EventClickedInfo 
    {
        [JsonProperty("clickedDateTime")]
        public DateTimeOffset ClickedDateTime { get; set; }
    }

    public class Data
    {
        [JsonProperty("events")]
        public List<EventItem> Events { get; set; }
    }
}
