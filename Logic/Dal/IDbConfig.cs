using System;

namespace Logic.Dal
{
    public interface IDbConfig
    {
        String ConnectionString { get; }
    }
}
