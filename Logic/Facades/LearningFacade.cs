using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Logic.Dal;
using Logic.Dal.Repositories;

namespace Logic.Facades
{
    public class LearningFacade
    {
        private readonly Lazy<IDataManagerFactrory> dataFactory;
        private readonly Lazy<ILog> logger;
 
        public LearningFacade(Func<IDataManagerFactrory> dataFactory, Func<ILoggerFactoryAdapter> loggerAdapter)
        {
            this.dataFactory = new Lazy<IDataManagerFactrory>(dataFactory);
            logger = new Lazy<ILog>(() => loggerAdapter().GetLogger(GetType()));
        }

        public async Task StartLearn()
        {
            var session = await Task.Run(() => InitSession());

            try
            {
                await Task.WhenAll(Task.Run(() => IterationStart(session)),
                    Task.Run(() => LearnIteration(session)));
            }
            finally
            {
                session.Completed = true;
            }
        }

        public async Task StopLearn()
        {
            
        }

        private LearningSession InitSession()
        {
            var calcTime = dataFactory.Value.WithRepository<TimeSpan?, ISettingsRepository>(repo => repo.GetCalcTime());
            return new LearningSession {CalcTime = calcTime ?? TimeSpan.Zero};
        }

        private void IterationStart(LearningSession session, CancellationToken token)
        {
            // создаем лернинг и отдаем на обработку
            while (!session.Completed)
            {
                token.ThrowIfCancellationRequested();

                var readTime = dataFactory.Value.WithRepository<TimeSpan?, ISettingsRepository>(
                    repo => repo.GetReadTime()) ?? TimeSpan.Zero;

                if (session.AllDone(readTime))
                {
                    session.Completed = true;
                    continue;
                }

                if (!session.NeedCalculate(readTime))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(30));
                    continue;
                }

                var learningId = dataFactory.Value.WithRepository<Int32, ILearningRepository>(
                    repo => repo.CreateLearning(session.DateFrom(), session.DateTo(readTime)));

                session.StartLearningCollection.Add(learningId, token);
            }

            session.StartLearningCollection.CompleteAdding();
        }

        private void LearnIteration(LearningSession session, CancellationToken token)
        {
            foreach (var learningId in session.StartLearningCollection.GetConsumingEnumerable(token))
            {
                token.ThrowIfCancellationRequested();

                var id = learningId;
                dataFactory.Value.WithRepository<ILearningRepository>(repo => repo.LearnIteration(id));
                session.FinishLearningCollection.Add(id, token);
            }

            session.FinishLearningCollection.CompleteAdding();
        }

        private void CompleteIteration(LearningSession session, CancellationToken token)
        {
            foreach (var learningId in session.FinishLearningCollection.GetConsumingEnumerable(token))
            {
                token.ThrowIfCancellationRequested();

                var id = learningId;
                dataFactory.Value.WithRepository<Double, ILearningRepository>(repo =>
                {
                    var likelihood = repo.LogLikelihood(id);
                }
                     );
            }
        }

        private class LearningSession
        {
            private readonly DateTime minDate = new DateTime(2014, 11, 17, 0, 27, 0);
            private readonly TimeSpan window = TimeSpan.FromMinutes(10);
            public BlockingCollection<Int32> StartLearningCollection { get; private set; } 
            public BlockingCollection<Int32> FinishLearningCollection { get; private set; } 

            public Int32 Id { get; set; }
            public TimeSpan TimeShift { get; set; }
            public Boolean Completed { get; set; }
            public TimeSpan CalcTime { get; set; }
            public TimeSpan AllTime { get; set; }

            public DateTime DateFrom()
            {
                return minDate + CalcTime;
            }
            public DateTime DateTo(TimeSpan readTime)
            {
                return DateFrom() + CalcTimeNow(readTime);
            }

            public LearningSession()
            {
                StartLearningCollection = new BlockingCollection<int>();
                FinishLearningCollection = new BlockingCollection<int>();

                Completed = false;
            }

            public Boolean NeedCalculate(TimeSpan readTime)
            {
                var calcAll = AllTime - CalcTime;
                var readNow = CalcTimeNow(readTime);
                return calcAll == readNow || readNow > window;
            }
            public TimeSpan CalcTimeNow(TimeSpan readTime)
            {
                var secondsToCalc = Math.Min((readTime - CalcTime).TotalSeconds, window.TotalSeconds);
                return TimeSpan.FromSeconds(secondsToCalc);                     
            }
            public Boolean AllDone(TimeSpan readTime)
            {
                return CalcTime == readTime && readTime == AllTime;
            }
        }
    }
}