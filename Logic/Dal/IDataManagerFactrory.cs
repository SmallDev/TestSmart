namespace Logic.Dal
{
    public interface IDataManagerFactrory
    {
        IDataManager GetDataManager();
    }
}