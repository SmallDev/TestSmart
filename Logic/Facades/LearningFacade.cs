using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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

        public LearningFacade(Func<IDataManagerFactrory> dataFactory, 
            Func<ILoggerFactoryAdapter> loggerAdapter)
        {
            this.dataFactory = new Lazy<IDataManagerFactrory>(dataFactory);
            logger = new Lazy<ILog>(() => loggerAdapter().GetLogger(GetType()));
        }

        public async Task StartLearning()
        {
            if (state != null)
            {
                logger.Value.Info("Learning is already run");
                return;
            }

            logger.Value.Info("Learning is started");
            try
            {
                state = await Task.Run(() => InitState());

                state.LearingTask = Task.WhenAll(
                    Task.Run(() => IterateLearning(state.Session, state.TokenSource.Token), state.TokenSource.Token),
                    Task.Run(() => UpdateLearning(state.Session, state.TokenSource.Token), state.TokenSource.Token),
                    Task.Run(() => UpdateSession(state.TokenSource.Token, state.Session), state.TokenSource.Token));

                await state.LearingTask;
                logger.Value.Info("Learning is completed");
            }
            catch (OperationCanceledException)
            {
                logger.Value.Info("Learning is cancelled");
            }
            catch (Exception)
            {
                BreakCurrentState().Wait();
                throw;
            }
        }
        public async Task StopLearning()
        {
            await BreakCurrentState();
            logger.Value.Info("Learning has been stopped");
        }
        
        public TimeSpan? GetCalcTime()
        {
            return state != null
                ? state.Session.CalcTime
                : dataFactory.Value.WithRepository<TimeSpan?, ISettingsRepository>(repo => repo.GetCalcTime());
        }
        public Double GetVelocity()
        {
            return state != null
                ? state.Session.Velocity
                : dataFactory.Value.WithRepository<Double, ISettingsRepository>(repo => repo.GetCalcVelocity() ?? 1.0);
        }
        public void SetVelocity(Double velocity)
        {
            if (state != null)
                state.Session.Velocity = velocity;

            dataFactory.Value.WithRepository<ISettingsRepository>(repo => repo.SetCalcVelocity(velocity), true);
            logger.Value.TraceFormat("CalcVelocity has been changed to {0}", velocity);
        }

        private LearningState InitState()
        {
            lock (critical)
            {
                var newState = new LearningState();

                dataFactory.Value.WithRepository<ISettingsRepository>(repo =>
                {
                    newState.Session.CalcTime = repo.GetCalcTime() ?? TimeSpan.Zero;
                    newState.Session.Velocity = repo.GetCalcVelocity() ?? 1.0;
                    newState.Session.AllTime = repo.GetAllTime() ?? TimeSpan.MaxValue;
                });

                logger.Value.InfoFormat("Init learning session: AllTime: {0}\tCalcTime: {1}\tVelocity: {2}",
                    newState.Session.AllTime, newState.Session.CalcTime, newState.Session.Velocity);

                return newState;
            }
        }

        private void IterateLearning(LearningSession session, CancellationToken token)
        {
            var sw = new Stopwatch();
            sw.Start();
            logger.Value.Trace("IterateLearning started");

            while (!session.Completed)
            {
                token.ThrowIfCancellationRequested();

                if (session.AllDone())
                    break;

                var readTime = dataFactory.Value.WithRepository<TimeSpan?, ISettingsRepository>(
                    repo => repo.GetReadTime()) ?? TimeSpan.Zero;

                while (!session.NeedCalculate(readTime))
                {
                    if (token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1)))
                        break;

                    readTime = dataFactory.Value.WithRepository<TimeSpan?, ISettingsRepository>(
                        repo => repo.GetReadTime()) ?? TimeSpan.Zero;
                }

                token.ThrowIfCancellationRequested();

                var learning = new Learning
                {                    
                    TimeTo = session.CalcShift(readTime),
                    CreatedOn = DateTime.Now
                };
                learning.TimeFrom = learning.TimeTo.Subtract(session.window);

                var savedLearning = dataFactory.Value.WithRepository<Learning, ILearningRepository>(repo => repo.Save(learning), true);
                logger.Value.TraceFormat("Learning {0}: Created from {1} to {2}", savedLearning.Id, savedLearning.TimeFrom, savedLearning.TimeTo);
                token.ThrowIfCancellationRequested();

                var sw1 = new Stopwatch();
                sw1.Start();
                dataFactory.Value.WithRepository<ILearningRepository>(repo => repo.InitUsers(savedLearning.Id));

                sw1.Stop();
                logger.Value.TraceFormat("Learning {0}: InitUsers elapsed {1}", savedLearning.Id, sw1.Elapsed);
                session.UpdateCollection.Add(savedLearning, token);

                sw1 = new Stopwatch();
                sw1.Start();
                dataFactory.Value.WithRepository<ILearningRepository>(repo => repo.LearnIteration(savedLearning.Id));
                
                sw1.Stop();
                logger.Value.TraceFormat("Learning {0}: Complete, elapsed {1}", savedLearning.Id, sw1.Elapsed);
                session.UpdateCollection.Add(savedLearning, token);

                session.CalcTime = savedLearning.TimeTo;
                dataFactory.Value.WithRepository<ISettingsRepository>(repo => repo.SetCalcTime(session.CalcTime), true);
                logger.Value.TraceFormat("CalcTime: moved to {0}\tstopWatch: {1}", session.CalcTime, session.StopWatch());
            }

            token.ThrowIfCancellationRequested();
            session.LearningCollection.CompleteAdding();

            sw.Stop();
            logger.Value.TraceFormat("IterateLearning completed. Elapsed {0}", sw.Elapsed);
        }

        private void UpdateLearning(LearningSession session, CancellationToken token)
        {
            foreach (var learning in session.UpdateCollection.GetConsumingEnumerable(token))
            {              
                var currentLearning = learning;

                var sw = new Stopwatch();
                sw.Start();
                dataFactory.Value.WithRepository<ILearningRepository>(repo =>
                {
                    var likelihood = repo.LogLikelihood(currentLearning.Id);
                    if (currentLearning.StartLikelihood == null)
                        currentLearning.StartLikelihood = likelihood;
                    else currentLearning.EndLikelihood = likelihood;

                    repo.Save(currentLearning);
                }, true);

                sw.Stop();
                logger.Value.TraceFormat("Learning {0}: Update, start={1}\tend={2}, elapsed {3}", 
                    currentLearning.Id, currentLearning.StartLikelihood, currentLearning.EndLikelihood, sw.Elapsed);
            }
        }
        private void UpdateSession(CancellationToken token, LearningSession session)
        {
            logger.Value.Trace("Update learning session started");

            while (!session.Completed)
            {
                token.ThrowIfCancellationRequested();

                if (token.WaitHandle.WaitOne(TimeSpan.FromSeconds(30)))
                    break;

                logger.Value.Trace("Update learning session");

                var prev = new { session.AllTime, session.Velocity };
                dataFactory.Value.WithRepository<ISettingsRepository>(repo =>
                {
                    session.Velocity = repo.GetCalcVelocity() ?? 1.0;
                });

                if (prev.AllTime != session.AllTime || Math.Abs(prev.Velocity - session.Velocity) > 1e-10)
                    logger.Value.InfoFormat("Learn settings has been changed: AllTime={0}\tVelocity={1}", session.AllTime,
                        session.Velocity);
            }

            token.ThrowIfCancellationRequested();
            logger.Value.Trace("Update learning session completed");
        }

        private async Task BreakCurrentState()
        {
            Task task;
            lock (critical)
            {
                if (state == null)
                    return;

                state.Session.Completed = true;
                state.TokenSource.Cancel();

                task = state.LearingTask;
                state = null;
            }

            try
            {
                await task;
            }
            catch (OperationCanceledException) { }
        }

        private class LearningState
        {
            public LearningSession Session { get; private set; }
            public CancellationTokenSource TokenSource { get; private set; }
            public Task LearingTask { get; set; }

            public LearningState()
            {
                Session = new LearningSession();
                TokenSource = new CancellationTokenSource();
            }
        }
        private class LearningSession
        {
            public readonly TimeSpan window = TimeSpan.FromMinutes(5);
            public BlockingCollection<Learning> LearningCollection { get; private set; }
            public BlockingCollection<Learning> UpdateCollection { get; private set; }

            public Boolean Completed { get; set; }
            public TimeSpan CalcTime { get; set; }
            public TimeSpan AllTime { get; set; }

            private TimeSpan elapsed;
            private DateTime? start;

            private Double velocity;
            private readonly Object stopWatchCritical = new Object();
            public Double Velocity
            {
                get { return velocity; }
                set
                {
                    if (Math.Abs(velocity - value) < 1e-3)
                        return;

                    lock (stopWatchCritical)
                    {
                        if (start.HasValue)
                        {
                            elapsed = StopWatch();
                            start = DateTime.Now;
                        }
                        velocity = value;
                    }
                }
            }

            public LearningSession()
            {
                LearningCollection = new BlockingCollection<Learning>();
                UpdateCollection = new BlockingCollection<Learning>();
                Completed = false;
                elapsed = TimeSpan.Zero;
            }

            public TimeSpan StopWatch()
            {
                if (start == null)
                    return TimeSpan.Zero;

                lock (stopWatchCritical)
                {
                    return elapsed + TimeSpan.FromSeconds((DateTime.Now - start.Value).TotalSeconds * Velocity);
                }
            }

            public Boolean NeedCalculate(TimeSpan readTime)
            {
                if (readTime < window)
                    return false;

                if (start == null)
                {
                    start = DateTime.Now;
                    elapsed = window;
                }

                return CalcShift(readTime) - CalcTime > TimeSpan.FromSeconds(window.TotalSeconds/10.0);
            }
           
            public TimeSpan CalcShift(TimeSpan readTime)
            {
                var availibleTime = MathExtension.Min(readTime, StopWatch());
                return MathExtension.Min(CalcTime + window, availibleTime);
            }

            public Boolean AllDone()
            {
                return CalcTime >= AllTime;
            }
        }
    }
}