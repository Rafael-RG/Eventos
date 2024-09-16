using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventos.Models
{
    public class ValidateSubscriptionRequest
    {
        public string UserEmail { get; set; }
    }

    public class SuscriberUserInfo
    {
        public string Email { get; set; }

        public bool IsSubscribed { get; set; }

        public int ClickCount { get; set; }

        public string Plan { get; set; }

        public DateTimeOffset PlanFinishDate { get; set; }
    }
}
