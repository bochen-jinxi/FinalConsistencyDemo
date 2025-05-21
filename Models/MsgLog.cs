using System;
using System.ComponentModel.DataAnnotations;

namespace FinalConsistencyDemo.Models
{
    public class MsgLog
    {
        [Key]
        public string LogKey { get; set; }  // 格式 "TranId-Account"
        public string TranId { get; set; }
        public string Account { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}