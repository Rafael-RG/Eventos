using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventos.Models
{
    public class LoginUser
    {
        public bool IsActive { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Country { get; set; }
        public int TotalClicks { get; set; }
        public int TotalClicksCurrentPeriod { get; set; }
        public long LastPeriod { get; set; }
    }
}
