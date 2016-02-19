using System;
using Logic.Dal;
using Logic.Model;
using MicrosoftResearch.Infer.Distributions;

namespace Logic.Facades
{
    public class StatisticsFacade
    {
        private readonly Func<IDataManagerFactrory> dataFactory;
        public StatisticsFacade(Func<IDataManagerFactrory> dataFactory)
        {
            this.dataFactory = dataFactory;
        }

        private Emulator emulator;
        public void StartEmulate()
        {
            emulator = new Emulator();
        }

        public Statistics ReadStatistics()
        {
            var result = new Statistics();
            if (emulator == null)
                return result;

            result.ReadPercentage = emulator.GenerateReadPercentage();
            result.CalculatePersentage = emulator.GenerateCalcPercentage();

            return result;
        }

        private class Emulator
        {
            private DateTime readTime;
            private DateTime calcTime;
            private Gaussian readVelocity;
            private Gaussian calcVelocity;

            private Double prevRead;
            private Double prevCalc;
            private TimeSpan calcLag;

            public Emulator()
            {
                readTime = DateTime.Now;
                calcTime = DateTime.Now;

                //readVelocity = new Gaussian(0.25, 0); // 25 persentages per min
                //calcVelocity = new Gaussian(0.2, 0); // 20 persentages per min
                readVelocity = new Gaussian(5, 0); // 25 persentages per min
                calcVelocity = new Gaussian(8, 0); // 20 persentages per min
                calcLag = TimeSpan.FromSeconds(3);
            }

            public Double GenerateReadPercentage()
            {
                var spentTime = (DateTime.Now - readTime);
                var pers = spentTime.TotalMinutes*readVelocity.Sample();
                if (pers > 0)
                    prevRead = Math.Min(prevRead + pers, 1);

                readTime += spentTime;
                return prevRead;
            }
            public Double GenerateCalcPercentage()
            {
                var spentTime = (DateTime.Now - calcTime) - calcLag;
                if (spentTime < TimeSpan.Zero)
                    return 0;

                var pers = spentTime.TotalMinutes*calcVelocity.Sample();
                if (pers > 0)
                    prevCalc = Math.Min(prevCalc + pers, prevRead);

                calcTime += spentTime;
                return prevCalc;
            }
        }
    }
}
