using System;
using System.Data;
using System.Data.SqlClient;
using Logic.Dal.NHibernate.Models;
using Logic.Dal.Repositories;
using Logic.Model;
using NHibernate;

namespace Logic.Dal.NHibernate.Repositories
{
    class LearingRepository : NHibernateRepositoryBase, ILearningRepository
    {
        private readonly Func<SqlConnection> connectionFunc;
        public LearingRepository(ISession session, Func<SqlConnection> connectionFunc)
            :base(session)
        {
            this.connectionFunc = connectionFunc;
        }

        public void InitClusters(Int32 clusterCount)
        {
            connectionFunc().WithCommand(command =>
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "dbo.InitClusters";
                command.Parameters.AddWithValue("count", clusterCount);
                command.CommandTimeout = (Int32) TimeSpan.FromMinutes(10).TotalSeconds;
                command.ExecuteNonQuery();
            });
        }

        public Learning Get(Int32 learningId)
        {
            return Session.Get<LearningDto>(learningId);
        }
        public Learning Save(Learning learning)
        {
            LearningDto dto = learning;
            Session.SaveOrUpdate(dto);
            return dto;
        }

        public void InitUsers(Int32 learningId)
        {
            connectionFunc().WithCommand(command =>
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "dbo.InitUsers";
                command.Parameters.AddWithValue("learning", learningId);
                command.CommandTimeout = (Int32)TimeSpan.FromMinutes(30).TotalSeconds;
                command.ExecuteNonQuery();
            });
        }

        public Double LogLikelihood(Int32 learningId)
        {
            return connectionFunc().WithCommand(command =>
            {
                command.CommandType = CommandType.Text;
                command.CommandText = "select dbo.CalcLogLikelihood(@learning)";
                command.Parameters.AddWithValue("learning", learningId);
                command.CommandTimeout = (Int32)TimeSpan.FromMinutes(30).TotalSeconds;
                return (Double)command.ExecuteScalar();
            });
        }

        public void LearnIteration(Int32 learningId)
        {
            connectionFunc().WithCommand(command =>
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "dbo.LearnIterate";
                command.Parameters.AddWithValue("learning", learningId);
                command.CommandTimeout = (Int32)TimeSpan.FromHours(1).TotalSeconds;
                command.ExecuteNonQuery();
            });
        }

        public void Clear()
        {
            connectionFunc().WithCommand(command =>
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "dbo.Clear";
                command.Parameters.AddWithValue("calcData", true);
                command.CommandTimeout = (Int32)TimeSpan.FromMinutes(30).TotalSeconds;
                command.ExecuteNonQuery();
            });
        }

        public void SaveStatistics(Int32 learningId)
        {
            connectionFunc().WithCommand(command =>
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "dbo.SaveStatistics";
                command.Parameters.AddWithValue("learning", learningId);
                command.CommandTimeout = (Int32)TimeSpan.FromMinutes(30).TotalSeconds;
                command.ExecuteNonQuery();
            });
        }
    }
}
