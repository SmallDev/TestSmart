using System.ComponentModel;
using Logic.Common;

namespace Logic.Model
{
    public enum MessageType
    {
        [Code("S")]
        [Description("Видеосигнал возобновлен")]
        S,

        [Code("V")]
        [Description("Пропадание видеосигнала")]
        V,

        [Code("K")]
        [Description("Периодическое сообщение (сигнал есть)")]
        K,

        [Code("Z")]
        [Description("Периодическое сообщение (сигнала все еще нет)")]
        Z,

        [Code("N")]
        [Description("Включение трансляции нового канала")]
        N,

        [Code("L")]
        [Description("Отключение трансляции канала")]
        L,

        [Code("Y")]
        [Description("Синхронизация для агрегатора, если 10 секунд нет событий")]
        Y,
    }
}