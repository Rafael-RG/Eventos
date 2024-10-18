using System.ComponentModel.DataAnnotations;

namespace Eventos.Models
{
    /// <summary>
    /// User details
    /// </summary>
    public class User : Bindableitem
    {
        private string fullName;
        private string email;
        private int totalClicks;
        private int totalClicksCurrentPeriod;
        private long lastPeriod;
        private bool isSubscribed;
        private int clickCount;
        private string plan;
        private DateTimeOffset planFinishDate;

        [Key]
        public string FullName
        {
            get => this.fullName;
            set
            {
                this.fullName = value;
                OnPropertyChanged(nameof(FullName));
            }
        }
        public string Email
        {
            get => this.email;
            set
            {
                this.email = value;
                OnPropertyChanged(nameof(Email));
            }

        }
        public string Country { get; set; }
        public int TotalClicks
        {
            get => this.totalClicks;
            set
            {
                this.totalClicks = value;
                OnPropertyChanged(nameof(TotalClicks));
            }
        }
        public int TotalClicksCurrentPeriod
        {
            get => this.totalClicksCurrentPeriod;
            set
            {
                this.totalClicksCurrentPeriod = value;
                OnPropertyChanged(nameof(TotalClicksCurrentPeriod));
            }
        }
        public long LastPeriod
        {
            get => this.lastPeriod;
            set
            {
                this.lastPeriod = value;
                OnPropertyChanged(nameof(LastPeriod));

            }
        }

        public bool IsSubscribed
        {
            get => this.isSubscribed;
            set
            {
                this.isSubscribed = value;
                OnPropertyChanged(nameof(IsSubscribed));
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(PlanInfo));
            }

        }
        public int ClickCount
        {
            get => this.clickCount;
            set
            {
                this.clickCount = value;
                OnPropertyChanged(nameof(ClickCount));
            }
        }

        public string Plan
        {
            get => this.plan;
            set
            {
                this.plan = value;
                OnPropertyChanged(nameof(Plan));
            }

        }

        public DateTimeOffset PlanFinishDate
        {
            get => this.planFinishDate;
            set
            {
                this.planFinishDate = value;
                OnPropertyChanged(nameof(PlanFinishDate));
            }

        }

        public string State
        {
            get => IsSubscribed ? "Suscripto" : "No suscripto";
        }

        public string PlanInfo
        {
            get => IsSubscribed ? $"{PlanFinishDate:dd/MM/yyyy}" : "N/A";
        }
    }
}
