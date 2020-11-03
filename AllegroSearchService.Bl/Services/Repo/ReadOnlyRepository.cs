using AllegroSearchService.Bl.ServiceInterfaces.Repo;
using AllegroSearchService.Data.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AllegroSearchService.Bl.Services.Repo
{
    public class ReadOnlyRepository<TContext> : IReadOnlyRepository where TContext : DbContext
    {
        protected readonly TContext _context;

        public ReadOnlyRepository(TContext context)
        {
            _context = context;
        }
        public IQueryable<TEntity> GetAllSync<TEntity>() where TEntity : class, IBaseEntity
        {
            return _context.Set<TEntity>();
        }

        protected virtual IQueryable<TEntity> PrepareQueryable<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? take = null)
            where TEntity : class, IBaseEntity
        {
            includeProperties = string.Empty;
            IQueryable<TEntity> query = _context.Set<TEntity>();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return query;
        }

        protected virtual IQueryable<TEntity> GetQueryable<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? take = null)
            where TEntity : class, IBaseEntity
        {
            return PrepareQueryable(filter, orderBy, includeProperties, skip, take).AsNoTracking();
        }

        public virtual async Task<ICollection<TEntity>> GetAll<TEntity>(
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? take = null)
            where TEntity : class, IBaseEntity
        {
            return await GetQueryable(null, orderBy, includeProperties, skip, take).ToListAsync()
                .ConfigureAwait(false);
        }

        public virtual async Task<TEntity> GetById<TEntity>(
            object id,
            string includeProperties = null)
            where TEntity : class, IBaseEntity
        {
            Expression<Func<TEntity, bool>> filter = x => x.Id == id;
            return await GetQueryable(filter, includeProperties: includeProperties).FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        public virtual async Task<ICollection<TEntity>> Get<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null,
            int? skip = null,
            int? take = null)
            where TEntity : class, IBaseEntity
        {
            return await GetQueryable(filter, orderBy, includeProperties, skip, take).ToListAsync()
                .ConfigureAwait(false);
        }

        public virtual async Task<TEntity> GetFirst<TEntity>(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>,
                IOrderedQueryable<TEntity>> orderBy = null,
            string includeProperties = null)
            where TEntity : class, IBaseEntity
        {
            return await GetQueryable(filter, orderBy, includeProperties).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public virtual async Task<int> GetCount<TEntity>(Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class, IBaseEntity
        {
            return await GetQueryable(filter).CountAsync().ConfigureAwait(false);
        }

        public virtual async Task<bool> CheckIfExist<TEntity>(Expression<Func<TEntity, bool>> filter = null)
            where TEntity : class, IBaseEntity
        {
            return await GetQueryable(filter).AnyAsync().ConfigureAwait(false);
        }
    }
}
