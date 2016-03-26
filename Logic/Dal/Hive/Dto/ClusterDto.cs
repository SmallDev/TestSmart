using System;

namespace Logic.Dal.Hive.Dto
{
    class ClusterDto
    {
        public Int32 ClusterId { get; set; }
        public Int32 ColumnId { get; set; }
        public Double Value { get; set; }
    }
}