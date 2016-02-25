using System;

namespace Logic.Dal.Repositories
{
    public interface ILearningRepository : IRepository
    {
        // Создание кластеров и их профилей
        void InitClusters(Int32 clusterCount);

        // Создание записи обучения
        Int32 CreateLearning(DateTime from, DateTime to);

        /// <summary> Обновление информации обучения </summary>
        void UpdateLearning(Int32 learningId, Double likelihood, Int32 iterationCount);

        // Создание пользоватей и их профилей по данным обучения
        void InitUsers(Int32 learningId);

        // Значение логарифма правдоподобия
        Double LogLikelihood(Int32 learningId);

        void LearnIteration(Int32 learningId);
    }
}