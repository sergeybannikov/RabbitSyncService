using System;

namespace Rabbit.Bridge.Messages
{
    public class RetryExample
    {
        public int RetryNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
