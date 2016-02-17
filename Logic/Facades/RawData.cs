using System;

namespace Logic.Facades
{
    public class RawData
    {
        public DateTime Timestamp { get; set; }

        public String Mac { get; set; }
        public String Date { get; set; }
        public String Time { get; set; }

        public String MessageType { get; set; }
        public String StreamType { get; set; }
    }
}