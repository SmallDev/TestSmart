using System;

namespace Logic.Model
{
    public class Learning
    {
        public Int32 Id { get; set; }
        public TimeSpan TimeFrom { get; set; }
        public TimeSpan TimeTo { get; set; }
        public Double? StartLikelihood { get; set; }
        public Double? EndLikelihood { get; set; }
        public Int32 Iterations { get; set; }
    }
}