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

        public LearningFacade(Func<IDataManagerFactrory> dataFactory, Func<ILoggerFactoryAdapter> loggerAdapter)
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
                    Task.Run(() => CreateLearning(state.Session, state.TokenSource.Token)),
                    Task.Run(() => IterateLearning(state.Session, state.TokenSource.Token)),
                    Task.Run(() => UpdateLearning(state.Session, state.TokenSource.Token)));

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

        private LearningState InitState()
        {
            lock (critical)
            {
                var newState = new LearningState();

                dataFactory.Value.WithRepository<ISettingsRepository>(repo =>
                {
                    newState.Session.CalcTime = repo.GetCalcTime() ?? TimeSpan.Zero;
                    newState.Session.AllTime = repo.GetAllTime() ?? TimeSpan.Zero;
                });

                logger.Value.InfoFormat("Init learning session: AllTime: {0}\tCalcTime: {1}",
                    newState.Session.AllTime, newState.Session.CalcTime);

                return newState;
            }
        }

        private void CreateLearning(LearningSession session, CancellationToken token)
        {
            var sw = new Stopwatch();
            sw.Start();
            logger.Value.Trace("CreateLearning started");
            Learning currentLearning = null;

            while (!session.Completed)
            {
                token.ThrowIfCancellationRequested();

                var readTime = dataFactory.Value.WithRepository<TimeSpan?, ISettingsRepository>(
                    repo => repo.GetReadTime()) ?? TimeSpan.Zero;

                if (session.AllDone())
                    break;
                
                while (!session.NeedCalculate(readTime))
                {
                    if (token.WaitHandle.WaitOne(TimeSpan.FromSeconds(1)))
                        break;
                }

                if (currentLearning != null)
                {
                    var id = currentLearning.Id;
                    currentLearning = dataFactory.Value.WithRepository<Learning, ILearningRepository>(repo => repo.Get(id));
                    while (session.NeedIterate(currentLearning))
                    {
                        if (token.WaitHandle.WaitOne(TimeSpan.FromSeconds(10)))
                            break;

                        currentLearning = dataFactory.Value.WithRepository<Learning, ILearningRepository>(repo => repo.Get(id));
                    }
                }

                var learning = new Learning
                {
                    TimeFrom = session.CalcTime,
                    TimeTo = session.CalcTime + session.CalcInterval(readTime),
                    Iterations = 0,
                    CreatedOn = DateTime.Now
                };

                currentLearning = dataFactory.Value.WithRepository<Learning, ILearningRepository>(repo => repo.Save(learning), true);
                logger.Value.TraceFormat("Learning {0}: Created", currentLearning.Id);

                session.LearningCollection.Add(currentLearning, token);
                //

                session.CalcTime = learning.TimeTo;
                dataFactory.Value.WithRepository<ISettingsRepository>(repo => repo.SetCalcTime(session.CalcTime), true);
            }

            token.ThrowIfCancellationRequested();
            session.LearningCollection.CompleteAdding();

            sw.Stop();
            logger.Value.TraceFormat("CreateLearning completed. Elapsed {0}", sw.Elapsed);
        }
        private void IterateLearning(LearningSession session, CancellationToken token)
        {           
            foreach (var learning in session.LearningCollection.GetConsumingEnumerable(token))
            {
                var learningId = learning.Id;

                var tmp = new Stopwatch();
                tmp.Start();
                dataFactory.Value.WithRepository<ILearningRepository>(repo => repo.InitUsers(learningId));

                tmp.Stop();
                logger.Value.TraceFormat("Learning {0}: InitUsers elapsed {1}", learningId, tmp.Elapsed);

                var currentLearning = learning;
                session.UpdateCollection.Add(currentLearning, token);

                while (session.NeedIterate(currentLearning))
                {
                    token.ThrowIfCancellationRequested();

                    var sw = new Stopwatch();
                    sw.Start();
                    dataFactory.Value.WithRepository<ILearningRepository>(repo => repo.LearnIteration(learningId));
                    sw.Stop();

                    currentLearning.Iterations++;
                    session.UpdateCollection.Add(currentLearning, token);

                    logger.Value.TraceFormat("Learning {0}: Complete #{1} elapsed {2}",
                        currentLearning.Id, currentLearning.Iterations, sw.Elapsed);

                    currentLearning = dataFactory.Value.WithRepository<Learning, ILearningRepository>(repo => repo.Get(learningId));
                }
            }
        }
        private void UpdateLearning(LearningSession session, CancellationToken token)
        {
            foreach (var learning in session.UpdateCollection.GetConsumingEnumerable(token))
            {
                var currentLearning = learning;

                dataFactory.Value.WithRepository<ILearningRepository>(repo =>
                {
                    var likelihood = repo.LogLikelihood(currentLearning.Id);
                    if (currentLearning.StartLikelihood == null)
                        currentLearning.StartLikelihood = likelihood;

                    currentLearning.EndLikelihood = likelihood;
                    repo.Save(currentLearning);
                }, true);

                logger.Value.TraceFormat("Learning {0}: Update #{1} start={2}\tend={3}", currentLearning.Id,
                    currentLearning.Iterations, currentLearning.StartLikelihood, currentLearning.EndLikelihood);
            }
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
            private readonly TimeSpan window = TimeSpan.FromMinutes(5);
            public BlockingCollection<Learning> LearningCollection { get; private set; }
            public BlockingCollection<Learning> UpdateCollection { get; private set; }

            public Boolean Completed { get; set; }
            public TimeSpan CalcTime { get; set; }
            public TimeSpan AllTime { get; set; }

            public LearningSession()
            {
                LearningCollection = new BlockingCollection<Learning>();
                UpdateCollection = new BlockingCollection<Learning>();
                Completed = false;
            }

            public Boolean NeedCalculate(TimeSpan readTime)
            {
                return CalcInterval(readTime) >= window || readTime == AllTime;
            }

            public Boolean NeedIterate(Learning learning)
            {
                if (DateTime.Now - learning.CreatedOn > window)
                    return false;

                if (learning.StartLikelihood == null || learning.EndLikelihood == null || learning.Iterations == 0)
                    return true;

                const Double lessPercentage = 0.2; // уменьшение правдоподобия на каждой итерации

                var f = Math.Pow(1 - lessPercentage, learning.Iterations);
                var d = learning.StartLikelihood*f;
                // выходим, если достигнута требуемая точность
                return learning.StartLikelihood * Math.Pow(1 - lessPercentage, learning.Iterations) < learning.EndLikelihood;
            }
            public TimeSpan CalcInterval(TimeSpan readTime)
            {
                return MathExtension.Min(readTime - CalcTime, window);
            }
            public Boolean AllDone()
            {
                return CalcTime == AllTime;
            }
        }
    }
}