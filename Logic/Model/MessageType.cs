using System.ComponentModel;
using Logic.Common;

namespace Logic.Model
{
    public enum MessageType
    {
        [Code("S")]
        [Description("����������� �����������")]
        S,

        [Code("V")]
        [Description("���������� ������������")]
        V,

        [Code("K")]
        [Description("������������� ��������� (������ ����)")]
        K,

        [Code("Z")]
        [Description("������������� ��������� (������� ��� ��� ���)")]
        Z,

        [Code("N")]
        [Description("��������� ���������� ������ ������")]
        N,

        [Code("L")]
        [Description("���������� ���������� ������")]
        L,

        [Code("Y")]
        [Description("������������� ��� ����������, ���� 10 ������ ��� �������")]
        Y,
    }
}