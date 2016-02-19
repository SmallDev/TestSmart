using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Logic.Model;

namespace Logic.Facades
{
    public class ReadSession
    {
        private readonly DateTime minDate = new DateTime(2014, 11, 17, 0, 27, 0);

        public BlockingCollection<RawData> RawData { get; private set; }
        public BlockingCollection<IList<Data>> ChunkData { get; private set; }

        public TimeSpan ReadTime { get; set; }
        public TimeSpan? AllTime { get; set; }
        
        private TimeSpan elapsed;
        private DateTime start;

        private Double velocity;
        private readonly Object velocityCritical = new Object();
        public Double Velocity
        {
            get { return velocity; }
            set
            {
                if (Math.Abs(velocity - value) < 1e-3)
                    return;

                lock (velocityCritical)
                {
                    elapsed = StopWatch();
                    start = DateTime.Now;
                    velocity = value;
                }                             
            }
        }

        public Boolean IsComplete { get; set; }

        public ReadSession()
        {
            IsComplete = false;
            RawData = new BlockingCollection<RawData>();
            ChunkData = new BlockingCollection<IList<Data>>();
            start = DateTime.Now;
            elapsed = TimeSpan.Zero;            
        }

        public TimeSpan StopWatch()
        {
            lock (velocityCritical)
            {
                return elapsed + TimeSpan.FromSeconds((DateTime.Now - start).TotalSeconds * Velocity);
            }            
        }
        public TimeSpan TimeShift(DateTime dateTime)
        {
            return TimeSpan.FromSeconds((dateTime - minDate).TotalSeconds);
        }

        public TimeSpan FutureRealTime(DateTime dateTime)
        {
            // увеличиваем время с запасом, чтобы накопились сообщения для обработки
            var delay = TimeSpan.FromSeconds(5*Velocity);
            var sessionTime = TimeShift(dateTime) - StopWatch();
            return TimeSpan.FromSeconds(sessionTime.TotalSeconds/Velocity) + delay; //масштабирование к реальному времени
        }
        public Boolean InPast(DateTime dateTime)
        {
            return TimeShift(dateTime) <= ReadTime;
        }
        public Boolean InFuture(DateTime dateTime)
        {
            // Сокращаем накопление в delay сек
            var delay = TimeSpan.FromSeconds(5*Velocity);
            return FutureRealTime(dateTime) > TimeSpan.Zero + delay;
        }

        public Boolean Finish(DateTime dateTime)
        {
            if (!AllTime.HasValue)
                return false;

            return TimeShift(dateTime) > AllTime.Value;
        }
    }
}