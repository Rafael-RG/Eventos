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
    }
}
