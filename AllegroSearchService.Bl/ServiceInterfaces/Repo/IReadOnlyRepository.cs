using AllegroSearchService.Data.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AllegroSearchService.Bl.ServiceInterfaces.Repo
{
    public interface IReadOnlyRepository
    {
        IQueryable<TEntity> GetAllSync<TEntity>() where TEntity : class, IBaseEntity;

        Task<ICollection<TEntity>> GetAll<TEntity>(
            Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? take = null)
            where TEntity : class, IBaseEntity;

        Task<ICollection<TEntity>> Get<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? take = null)
            where TEntity : class, IBaseEntity;

        Task<TEntity> GetById<TEntity>(
            object id,
            string includeProperties = null)
            where TEntity : class, IBaseEntity;

        Task<TEntity> GetFirst<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>,
            IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null)
            where TEntity : class, IBaseEntity;

        Task<int> GetCount<TEntity>(
            Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class, IBaseEntity;

        Task<bool> CheckIfExist<TEntity>(
            Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class, IBaseEntity;
    }
}
