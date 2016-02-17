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

        public DateTime Start { get; private set; }
        public TimeSpan ReadTime { get; set; }
        public TimeSpan? AllTime { get; set; }
        public Double Velocity { get; set; }

        public ReadSession()
        {
            RawData = new BlockingCollection<RawData>();
            ChunkData = new BlockingCollection<IList<Data>>();
            Start = DateTime.Now;
        }

        public TimeSpan StopWatch()
        {
            return TimeSpan.FromSeconds((DateTime.Now - Start).TotalSeconds*Velocity);
        }
        public TimeSpan TimeShift(DateTime dateTime)
        {
            return TimeSpan.FromSeconds((dateTime - minDate).TotalSeconds*Velocity);
        }

        public TimeSpan FutureTime(DateTime dateTime)
        {
            // увеличиваем время с запасом, чтобы накопились сообщения для обработки
            var delay = TimeSpan.FromSeconds(5/Velocity);
            return TimeShift(dateTime) - StopWatch() + delay;
        }
        public Boolean InPast(DateTime dateTime)
        {
            return TimeShift(dateTime) < ReadTime;
        }
        public Boolean InFuture(DateTime dateTime)
        {
            // Сокращаем накопление в delay сек
            var delay = TimeSpan.FromSeconds(5/Velocity);
            return FutureTime(dateTime) > TimeSpan.Zero + delay;
        }

        public Boolean Finish(DateTime dateTime)
        {
            if (!AllTime.HasValue)
                return false;

            return TimeShift(dateTime) > AllTime.Value;
        }
    }
}