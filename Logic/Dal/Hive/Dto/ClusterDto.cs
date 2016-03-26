using System;
using System.Collections.Generic;
using Logic.Model;

namespace Logic.Dal.Hive.Dto
{
    class ClusterDto
    {
        public static Dictionary<Int32, PropertyType> types = new Dictionary<Int32, PropertyType>
        {
            {1, PropertyType.Received},
            {2, PropertyType.LinkFaults},
            {3, PropertyType.Restored},
            {4, PropertyType.Overflow},
            {5, PropertyType.Underflow},
            {6, PropertyType.UpTime},
            {7, PropertyType.VidDecodeErrors},
            {8, PropertyType.VidDataErrors},
            {9, PropertyType.AvTimeSkew},
            {10, PropertyType.AvPeriodSkew},
            {11, PropertyType.BufUnderruns},
            {12, PropertyType.BufOverruns},
            {13, PropertyType.DvbLevel},
            {14, PropertyType.CurBitrate}
        };

        public Int32 ClusterId { get; set; }
        public Int32 ColumnId { get; set; }
        public Double Value { get; set; }
    }
}