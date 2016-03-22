using System;
using Logic.Model;

namespace Logic.Dal.Repositories
{
    public interface ILearningRepository : IRepository
    {
        // Создание кластеров и их профилей
        void InitClusters(Int32 count);

        Learning Get(Int32 learningId);
        Learning Save(Learning learning);

        // Создание пользоватей и их профилей по данным обучения
        void InitUsers(Int32 learningId);

        // Значение логарифма правдоподобия
        Double LogLikelihood(Int32 learningId);

        void LearnIteration(Int32 learningId);

        // Очистка результатов вычислений и промежуточных данных
        void Clear();

        // Сохранить промежуточную статистику обучения
        void SaveStatistics(Int32 learningId);
    }
}