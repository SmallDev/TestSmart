using System;

namespace Logic.Model
{
    public class Data
    {
        public TimeSpan Timestamp { get; set; }
        public String Mac { get; set; }

        public MessageType? MessageType { get; set; }
        public StreamType? StreamType { get; set; }
        public Double? ReceivedRate { get; set; }
    }
}