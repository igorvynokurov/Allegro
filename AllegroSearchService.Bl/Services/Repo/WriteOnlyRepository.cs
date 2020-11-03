using AllegroSearchService.Bl.ServiceInterfaces.Repo;
using AllegroSearchService.Data.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AllegroSearchService.Bl.Services
{
    public class WriteOnlyRepository<TContext> : IWriteOnlyRepository, IDisposable where TContext : DbContext
    {
        private readonly TContext _context;

        private IList<IBaseEntity> _addedEntities { get; }

        private bool _disposed;

        public WriteOnlyRepository(
            TContext context)
        {
            _context = context;
            _addedEntities = new List<IBaseEntity>();
        }

        public virtual void Add<TEntity>(
            TEntity entity,
            Guid createdById)
            where TEntity : class, IBaseEntity
        {
            entity.CreateDate = DateTime.UtcNow;
            entity.CreatedBy = createdById;
            entity.ModifiedDate = DateTime.UtcNow;
            entity.ModifiedBy = createdById;
            _context.Set<TEntity>().Add(entity);

            _addedEntities.Add(entity);
        }

        public virtual void Update<TEntity>(TEntity entity,
            Guid modifiedById)
            where TEntity : class, IBaseEntity
        {
            entity.ModifiedDate = DateTime.UtcNow;
            entity.ModifiedBy = modifiedById;

            var trackedEntity = GetTrackedEntity(entity);

            if (trackedEntity == null)
            {
                _context.Entry(entity).State = EntityState.Modified;
            }

            else if (trackedEntity.State != EntityState.Added)
            {
                trackedEntity.State = EntityState.Modified;
            }
        }

        public virtual void Delete<TEntity>(TEntity entity,
            Guid deletedById)
            where TEntity : class, IBaseEntity
        {
            var trackedEntity = GetTrackedEntity(entity);

            if (trackedEntity == null)
            {
                _context.Entry(entity).State = EntityState.Deleted;
            }
            else if (trackedEntity.State != EntityState.Added)
            {
                trackedEntity.State = EntityState.Deleted;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        public virtual async Task Save()
        {
            ValidateContext();
            ResolveExistingAddedEntries(_context);
            try
            {
                await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var detailedConcurrencyEx = new DbUpdateConcurrencyException("Concurrency exception!", ex);
                throw detailedConcurrencyEx;
            }

            DetachAndDeleteSavedEntities();
        }

        public void Commit()
        {
            _context.SaveChanges();
        }

        protected void ValidateContext()
        {
            var entities = _context.ChangeTracker.Entries().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified)
                .Select(x => x.Entity);

            var validationResults = new List<ValidationResult>();

            foreach (var entity in entities)
            {
                if (!Validator.TryValidateObject(entity, new ValidationContext(entity), validationResults))
                {
                    var fullErrorMessage = string.Join("; ", validationResults.Select(x => x.ErrorMessage));
                    throw new ValidationException(fullErrorMessage);
                }
            }
        }

        protected EntityEntry<TEntity> GetTrackedEntity<TEntity>(TEntity entity)
            where TEntity : class, IBaseEntity
        {
            var trackedEntity = _context.ChangeTracker.Entries<TEntity>().SingleOrDefault(e => e.Entity.Id.Equals(entity.Id));
            return trackedEntity;
        }

        private void ResolveExistingAddedEntries(DbContext context)
        {
            var addedEntities = context.ChangeTracker.Entries().Where(x => x.State == EntityState.Added);
            foreach (var addedEntitity in addedEntities)
            {
                var baseEntity = addedEntitity.Entity as IBaseEntity;

                if (_addedEntities.Contains(baseEntity))
                {
                    continue;
                }

                addedEntitity.State = EntityState.Unchanged;
            }
        }

        private void DetachAndDeleteSavedEntities()
        {
            var changedEntriesCopy = _context.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Unchanged).ToList();

            foreach (var entry in changedEntriesCopy)
            {
                entry.State = EntityState.Detached;
            }

            _addedEntities.Clear();
        }
    }
}
