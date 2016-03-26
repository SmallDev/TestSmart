using Logic.Common;

namespace Logic.Model
{
    public enum PropertyType
    {
        [Description("Received", typeof(Resources.Property))]
        Received,

        [Description("LINK_FAULTS", typeof(Resources.Property))]
        LinkFaults,

        [Description("RESTORED", typeof(Resources.Property))]
        Restored,

        [Description("OVERFLOW", typeof(Resources.Property))]
        Overflow,

        [Description("UNDERFLOW", typeof(Resources.Property))]
        Underflow,

        [Description("UPTIME", typeof(Resources.Property))]
        UpTime,

        [Description("VID_DECODE_ERRORS", typeof(Resources.Property))]
        VidDecodeErrors,

        [Description("VID_DATA_ERRORS", typeof(Resources.Property))]
        VidDataErrors,

        [Description("AV_TIME_SKEW", typeof(Resources.Property))]
        AvTimeSkew,

        [Description("AV_PERIOD_SKEW", typeof(Resources.Property))]
        AvPeriodSkew,

        [Description("BUF_UNDERRUNS", typeof(Resources.Property))]
        BufUnderruns,

        [Description("BUF_OVERRUNS", typeof(Resources.Property))]
        BufOverruns,

        [Description("DVB_LEVEL", typeof(Resources.Property))]
        DvbLevel,

        [Description("CUR_BITRAT", typeof(Resources.Property))]
        CurBitrate,

        [Description("LOST", typeof(Resources.Property))]
        Lost,

        [Description("MDI_DF", typeof(Resources.Property))]
        DelayFactor,

        [Description("MDI_MLR", typeof(Resources.Property))]
        MediaLossRate
    }
}