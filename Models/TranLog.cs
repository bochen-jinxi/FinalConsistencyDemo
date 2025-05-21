using System;
using System.ComponentModel.DataAnnotations;

namespace FinalConsistencyDemo.Models
{
    public class TranLog
    {
        [Key]
        public string TranId { get; set; }
        public string FromAccount { get; set; }
        public string ToAccount { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}