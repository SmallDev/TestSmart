using Logic.Common;

namespace Logic.Model
{
    public enum PropertyType
    {
        [Description("Received", typeof(Resources.Property))]
        Received = 1,

        [Description("LINK_FAULTS", typeof(Resources.Property))]
        LinkFaults = 2,

        [Description("RESTORED", typeof(Resources.Property))]
        Restored = 3,

        [Description("OVERFLOW", typeof(Resources.Property))]
        Overflow = 4,

        [Description("UNDERFLOW", typeof(Resources.Property))]
        Underflow = 5,

        [Description("UPTIME", typeof(Resources.Property))]
        UpTime = 6,

        [Description("VID_DECODE_ERRORS", typeof(Resources.Property))]
        VidDecodeErrors = 7,

        [Description("VID_DATA_ERRORS", typeof(Resources.Property))]
        VidDataErrors = 8,

        [Description("AV_TIME_SKEW", typeof(Resources.Property))]
        AvTimeSkew = 9,

        [Description("AV_PERIOD_SKEW", typeof(Resources.Property))]
        AvPeriodSkew = 10,

        [Description("BUF_UNDERRUNS", typeof(Resources.Property))]
        BufUnderruns = 11,

        [Description("BUF_OVERRUNS", typeof(Resources.Property))]
        BufOverruns = 12,

        [Description("DVB_LEVEL", typeof(Resources.Property))]
        DvbLevel = 13,

        [Description("CUR_BITRAT", typeof(Resources.Property))]
        CurBitrate = 14
    }
}