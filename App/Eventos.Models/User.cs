using System.ComponentModel.DataAnnotations;

namespace Eventos.Models
{
    /// <summary>
    /// User details
    /// </summary>
    public class User
    {
        [Key]
        public string FullName { get; set; }
        public string Email { get; set; }
    }
}
