using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Models
{
    public class Feedback
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTimeOffset Created { get; set; }
    }

    public class FeedbackEntry : ITableEntity
    {
        public string Email { get; set; }
        public string Message { get; set; }
        public bool IsResolve { get; set; }
        public DateTimeOffset Created { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
