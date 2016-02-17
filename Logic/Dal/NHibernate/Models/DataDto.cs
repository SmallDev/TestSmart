using System;
using Logic.Model;

namespace Logic.Dal.NHibernate.Models
{
    internal class DataDto
    {
        public virtual Int64 Id { get; set; }
        public virtual DateTime Timestamp { get; set; }
        public virtual String Mac { get; set; }
        public virtual MessageType? MessageType { get; set; }
        public virtual ContentType? ContentType { get; set; }

        public static DataDto Create(Data data)
        {
            return new DataDto
            {
                Timestamp = data.Timestamp,
                Mac = data.Mac,
                MessageType = data.MessageType,
                ContentType = data.ContentType,
            };
        }
    }    
}