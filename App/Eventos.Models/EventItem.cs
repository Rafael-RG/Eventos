using Newtonsoft.Json;

namespace Eventos.Models
{
    public class EventItem : Bindableitem
    {
        private string title;
        private string description;
        private DateTime date;
        private string zone;
        private string startTime;
        private string endTime;
        private string eventURl;
        private string count;
        private List<EventClickedInfo> clickedInfo;

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
        public DateTime Date
        {
            get => this.date;
            set
            {
                this.date = value;
                OnPropertyChanged(nameof(Date));
            }
        }
        [JsonProperty("zone")]
        public string Zone
        {
            get => this.zone;
            set
            {
                this.zone = value;
                OnPropertyChanged(nameof(Zone));
            }
        }

        [JsonProperty("zoneId")]
        public string ZoneId { get; set; }



        [JsonProperty("startTime")]
        public string StartTime
        {
            get => this.startTime;
            set
            {
                this.startTime = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }

        [JsonProperty("endTime")]
        public string EndTime
        {
            get => this.endTime;
            set
            {
                this.endTime = value;
                OnPropertyChanged(nameof(EndTime));
            }
        }

        public string URLFile { get; set; }

        [JsonProperty("rowKey")]
        public string RowKey { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty("count")]
        public string Count
        {
            get => this.count;
            set
            {
                this.count = value;
                OnPropertyChanged(nameof(Count));
            }
        }

        [JsonProperty("eventURL")]
        public string EventURl
        {
            get => this.eventURl;
            set
            {
                this.eventURl = value;
                OnPropertyChanged(nameof(EventURl));
            }
        }

        [JsonProperty("isDelete")]
        public bool IsDelete { get; set; }

        [JsonProperty("clickedInfo")]
        public List<EventClickedInfo> ClickedInfo
        {
            get => this.clickedInfo;
            set
            {
                this.clickedInfo = value;
                OnPropertyChanged(nameof(ClickedInfo));
            }
        }

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
