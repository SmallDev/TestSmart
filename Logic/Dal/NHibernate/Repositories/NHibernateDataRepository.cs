using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Dal.NHibernate.Models;
using Logic.Dal.Repositories;
using Logic.Model;
using NHibernate;
using NHibernate.Util;

namespace Logic.Dal.NHibernate.Repositories
{
    class NHibernateDataRepository : NHibernateRepositoryBase, IDataRepository
    {
        public NHibernateDataRepository(ISession session)
            : base(session)
        {
        }

        public void Save(IList<Data> data)
        {
            data.Select(DataDto.Create).ForEach(d => Session.Save(d));
        }

        public void Clear()
        {
            var query = Session.CreateSQLQuery("TRUNCATE TABLE Data");
            query.SetTimeout(TimeSpan.FromMinutes(10).Seconds);
            query.ExecuteUpdate();
        }
    }
}