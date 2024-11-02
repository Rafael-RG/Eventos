using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eventos.Models
{
    public class Feedback
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTimeOffset Created { get; set; }
    }
}
