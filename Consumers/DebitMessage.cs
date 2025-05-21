namespace FinalConsistencyDemo.Consumers
{
    public class DebitMessage
    {
        public string TranId { get; set; }
        public string Account { get; set; }
        public int Delta { get; set; }
    }
}
