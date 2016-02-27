using System;
using System.Data.SqlClient;
using Logic.Dal.Repositories;

namespace Logic.Dal.NHibernate.Repositories
{
    class SqlLearingRepository : ILearningRepository
    {
        private readonly Func<SqlConnection> connectionFunc;
        public SqlLearingRepository(Func<SqlConnection> connectionFunc)
        {
            this.connectionFunc = connectionFunc;
        }

        public int StartLearning(DateTime @from, DateTime to)
        {
            throw new NotImplementedException();
        }

        public void CompleteLearning(int learningId)
        {
            throw new NotImplementedException();
        }

        public double UpdateLogLikelihood(int learningId)
        {
            throw new NotImplementedException();
        }

        public void UpdateProfiles(int learningId)
        {
            throw new NotImplementedException();
        }

        public void UpdateLatent(int learningId)
        {
            throw new NotImplementedException();
        }
    }
}
