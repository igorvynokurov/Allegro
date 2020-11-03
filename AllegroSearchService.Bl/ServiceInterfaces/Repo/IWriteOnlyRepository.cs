using AllegroSearchService.Data.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AllegroSearchService.Bl.ServiceInterfaces.Repo
{
    public interface IWriteOnlyRepository
    {
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="createdById"></param>
        void Add<TEntity>(
            TEntity entity,
            Guid createdById)
            where TEntity : class, IBaseEntity;

        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="modifiedById"></param>
        void Update<TEntity>(
            TEntity entity,
            Guid modifiedById)
            where TEntity : class, IBaseEntity;


        void Delete<TEntity>(
            TEntity entity,
            Guid deletedById)
            where TEntity : class, IBaseEntity;

        Task Save();
        void Commit();
    }
}
