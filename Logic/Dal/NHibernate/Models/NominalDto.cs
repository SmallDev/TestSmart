using System;

namespace Logic.Dal.NHibernate.Models
{
    public class NominalDto
    {
        public virtual Int32 Id { get; set; }
        public virtual Int32 ColumnId { get; set; }        
        public virtual String Value { get; set; }
    }
}