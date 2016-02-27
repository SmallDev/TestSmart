using System;

namespace Logic.Dal.Repositories
{
    public interface ILearningRepository : IRepository
    {
        // Вычисление временных таблиц n и H
        Int32 StartLearning(DateTime from, DateTime to);

        // Удаление временных таблиц
        void CompleteLearning(Int32 learningId);

        // Значение логарифма правдоподобия
        Double UpdateLogLikelihood(Int32 learningId);

        // Обновить профили пользователей и кластеров
        void UpdateProfiles(Int32 learningId);

        // Обновить таблицы скрытых переменных
        void UpdateLatent(Int32 learningId);
    }
}