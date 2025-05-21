using System;

namespace FinalConsistencyDemo.Models
{
    public class MessageQueue
    {
        public long Id { get; set; }
        public string TranId { get; set; }
        public string Account { get; set; }
        public decimal Delta { get; set; }
        public string Type { get; set; } // "debit" 或 "credit"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}