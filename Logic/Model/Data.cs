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
        public Double? LinkFaultsRate { get; set; }
        public Double? LostRate { get; set; }
        public Double? RestoredRate { get; set; }
        public Double? OverflowRate { get; set; }
        public Double? UnderflowRate { get; set; }
        public Double? DelayFactor { get; set; }
        public Double? MediaLossRate { get; set; }
    }
}