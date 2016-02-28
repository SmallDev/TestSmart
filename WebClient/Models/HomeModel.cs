using System;

namespace WebClient.Models
{
    public class ControlModel
    {
        public Double Velocity { get; set; }
        public String AllTime { get; set; }
        public String ReadTime { get; set; }
        public String CalcTime { get; set; }

        public void SetAllTime(TimeSpan? time)
        {
            AllTime = TimeFormat(time);
        }
        public void SetReadTime(TimeSpan? time)
        {
            ReadTime = TimeFormat(time);
        }
        public void SetCalcTime(TimeSpan? time)
        {
            CalcTime = TimeFormat(time);
        }

        private String TimeFormat(TimeSpan? time)
        {
            return !time.HasValue ? String.Empty : time.Value.ToString("hh\\:mm\\:ss");
        }
    }
}