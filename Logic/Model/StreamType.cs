using System.ComponentModel;
using Logic.Common;

namespace Logic.Model
{
    [Description("��� ������")]
    public enum StreamType
    {
        [Code("M")]
        [Description("Multicast")]
        M,

        [Code("U")]
        [Description("Unicast")]
        U,
    }
}