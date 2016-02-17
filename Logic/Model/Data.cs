using System;

namespace Logic.Model
{
    public class Data
    {
        public DateTime Timestamp { get; set; }
        public String Mac { get; set; }

        public MessageType? MessageType { get; set; }
        public ContentType? ContentType { get; set; }
    }

    public enum MessageType
    {
        S
    }

    public enum ContentType
    {
        C
    }
}