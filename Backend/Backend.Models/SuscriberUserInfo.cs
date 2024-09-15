using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class SuscriberUserInfo
    {
        public string Email { get; set; }

        public bool IsSubscribed { get; set; }

        public int ClickCount { get; set; }

        public string Plan { get; set; }

        public DateTimeOffset PlanFinishDate { get; set; }
    }
}
