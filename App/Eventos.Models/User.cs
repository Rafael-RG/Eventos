using System.ComponentModel.DataAnnotations;

namespace Eventos.Models
{
    /// <summary>
    /// User details
    /// </summary>
    public class User:Bindableitem
    {
        private string fullName;

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
        public string Email { get; set; }
        public string Country { get; set; }
        public int TotalClicks { get; set; }
        public int TotalClicksCurrentPeriod { get; set; }
        public long LastPeriod { get; set; }

        public bool IsSubscribed { get; set; }

        public int ClickCount { get; set; }

        public string Plan { get; set; }

        public DateTimeOffset PlanFinishDate { get; set; }
    }
}
