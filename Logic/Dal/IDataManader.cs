namespace Logic.Dal
{
    public interface IDataManader
    {
        T GetRepository<T>() where T:IRepository;
    }
}
