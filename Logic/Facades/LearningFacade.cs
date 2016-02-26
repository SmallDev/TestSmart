using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Logic.Dal;
using Logic.Dal.Repositories;
using Logic.Model;

namespace Logic.Facades
{
    public class LearningFacade
    {
        private readonly Lazy<IDataManagerFactrory> dataFactory;
        private readonly Lazy<ILog> logger;

        private LearningState state;
        private readonly Object critical = new Object();

        public LearningFacade(Func<IDataManagerFactrory> dataFactory, Func<ILoggerFactoryAdapter> loggerAdapter)
        {
            this.dataFactory = new Lazy<IDataManagerFactrory>(dataFactory);
            logger = new Lazy<ILog>(() => loggerAdapter().GetLogger(GetType()));
        }

        public async Task StartLearn()
        {
            try
            {
                await Task.Run(() => InitState());

                await Task.Run(() => Learning(state.Session, state.TokenSource.Token));
            }
            catch (Exception ex)
            {
                state.TokenSource.Cancel();
                logger.Value.Error(ex.Message, ex);
                throw;
            }
            finally
            {
                state.Session.Completed = true;
            }
        }
        public async Task StopLearn()
        {
            await Task.Run(() => BreakCurrentState());
        }

        private void InitState()
        {
            lock (critical)
            {
                BreakCurrentState();

                var newState = new LearningState();

                dataFactory.Value.WithRepository<ISettingsRepository>(repo =>
                {
                    newState.Session.CalcTime = repo.GetCalcTime() ?? TimeSpan.Zero;
                    newState.Session.AllTime = repo.GetAllTime() ?? TimeSpan.Zero;
                });

                state = newState;
            }
        }

        private void Learning(LearningSession session, CancellationToken token)
        {
            CancellationTokenSource currentLearning = null;
            Learning learning;
            // создаем лернинг и отдаем на обработку
            while (!session.Completed && !token.IsCancellationRequested)
            {
                var readTime = dataFactory.Value.WithRepository<TimeSpan?, ISettingsRepository>(
                    repo => repo.GetReadTime()) ?? TimeSpan.Zero;

                //if (session.AllDone(readTime))
                //{
                //    session.Completed = true;
                //    continue;
                //}

                //if (!session.NeedCalculate(readTime))
                //{
                //    Thread.Sleep(TimeSpan.FromSeconds(30));
                //    continue;
                //}

                if (currentLearning != null)
                {
                    currentLearning.Cancel();
                }

                currentLearning = new CancellationTokenSource();
                var currentToken = currentLearning.Token;

                learning = new Learning
                {
                    TimeFrom = session.CalcTime,
                    TimeTo = session.CalcTime + session.CalcTimeNow(readTime),
                    Iterations = 0
                };
                
               // Task.Run(() => Start(learning, session, currentToken), currentToken);
                Start(learning, session, currentToken);
            }

            if (currentLearning != null)
                currentLearning.Cancel();
        }
        private void Start(Learning learning, LearningSession session, CancellationToken token)
        {
            dataFactory.Value.WithRepository<ILearningRepository>(repo => learning = repo.Save(learning), true);
            token.ThrowIfCancellationRequested();

            dataFactory.Value.WithRepository<ILearningRepository>(repo => repo.InitUsers(learning.Id));
            token.ThrowIfCancellationRequested();

            dataFactory.Value.WithRepository<ILearningRepository>(repo =>
            {
                learning.StartLikelihood = repo.LogLikelihood(learning.Id);
                learning.Iterations = 0;
                learning = repo.Save(learning);
            }, true);

            while (!token.IsCancellationRequested)
            {
                dataFactory.Value.WithRepository<ILearningRepository>(repo => repo.LearnIteration(learning.Id));
                dataFactory.Value.WithRepository<ILearningRepository>(repo =>
                {
                    learning.EndLikelihood = repo.LogLikelihood(learning.Id);
                    learning.Iterations ++;
                    learning = repo.Save(learning);
                }, true);
            }
        }

        private void BreakCurrentState()
        {
            lock (critical)
            {
                if (state == null)
                    return;

                state.Session.Completed = true;
                state.TokenSource.Cancel();
                state = null;
            }
        }

        private class LearningState
        {
            public LearningSession Session { get; private set; }
            public CancellationTokenSource TokenSource { get; private set; }

            public LearningState()
            {
                Session = new LearningSession();
                TokenSource = new CancellationTokenSource();
            }
        }
        private class LearningSession
        {
            private readonly TimeSpan window = TimeSpan.FromMinutes(10);
            public BlockingCollection<Int32> StartLearningCollection { get; private set; } 
            public BlockingCollection<Int32> FinishLearningCollection { get; private set; } 

            public Int32 Id { get; set; }
            public TimeSpan TimeShift { get; set; }
            public Boolean Completed { get; set; }
            public TimeSpan CalcTime { get; set; }
            public TimeSpan AllTime { get; set; }

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