using System.ComponentModel.DataAnnotations;

namespace FinalConsistencyDemo.Models
{
    public class Account
    {
        [Key]
        public string AccountId { get; set; }
        public decimal Balance { get; set; }
    }
}