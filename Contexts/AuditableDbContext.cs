using Extensions;
using Models;
using Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Diagnostics.CodeAnalysis;

namespace Contexts
{
    public class AuditableDbContext : IdentityDbContext<IdentityUser>
    {
        private readonly IAuthenticatedUserService _authenticatedUser;
        private readonly IDateTimeService _dateTimeService;


        public AuditableDbContext(
            IAuthenticatedUserService authenticatedUser,
            IDateTimeService dateTimeService,
            DbContextOptions options)
          : base(options)
        {
            _authenticatedUser = authenticatedUser;
            _dateTimeService = dateTimeService;
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // apply filters
            builder.ApplyGlobalFilters<IAuditable>(e => e.IsDeleted == null || e.IsDeleted == false);
        }

        public override EntityEntry<TEntity> Add<TEntity>(TEntity entity) where TEntity : class
        {
            // set some data
            if (entity != null && entity is IAuditable)
            {
                ((IAuditable)entity).CreatedAt = _dateTimeService.NowUtc;
                ((IAuditable)entity).CreatedById = _authenticatedUser.UserId;
                ((IAuditable)entity).IsDeleted = false;
                ((IAuditable)entity).IsModified = false;
            }

            return base.Add<TEntity>(entity);
        }

        public override EntityEntry Add(object entity)
        {
            // set some data
            if (entity != null && entity is IAuditable)
            {
                ((IAuditable)entity).CreatedAt = _dateTimeService.NowUtc;
                ((IAuditable)entity).CreatedById = _authenticatedUser.UserId;
                ((IAuditable)entity).IsDeleted = false;
                ((IAuditable)entity).IsModified = false;
            }

            return base.Add(entity);
        }

        public override EntityEntry<TEntity> Attach<TEntity>(TEntity entity) where TEntity : class
        {
            var result = base.Attach(entity);

            if (entity is IAuditable)
            {
                ((IAuditable)result.Entity).IsModified = true;
                ((IAuditable)result.Entity).ModifiedAt = _dateTimeService.NowUtc;
                ((IAuditable)result.Entity).ModifiedById = _authenticatedUser.UserId;

                Entry((IAuditable)entity).Property(e => e.IsModified).IsModified = true;
                Entry((IAuditable)entity).Property(e => e.ModifiedAt).IsModified = true;
                Entry((IAuditable)entity).Property(e => e.ModifiedById).IsModified = true;
            }

            return result;
        }

        public override EntityEntry Attach(object entity)
        {
            var result = base.Attach(entity);

            if (entity is IAuditable)
            {
                ((IAuditable)result.Entity).IsModified = true;
                ((IAuditable)result.Entity).ModifiedAt = _dateTimeService.NowUtc;
                ((IAuditable)result.Entity).ModifiedById = _authenticatedUser.UserId;

                Entry((IAuditable)entity).Property(e => e.IsModified).IsModified = true;
                Entry((IAuditable)entity).Property(e => e.ModifiedAt).IsModified = true;
                Entry((IAuditable)entity).Property(e => e.ModifiedById).IsModified = true;
            }

            return result;
        }

        public override EntityEntry Remove(object entity)
        {
            return Remove(entity, true);
        }

        public EntityEntry Remove([NotNull] object entity, bool isSoftDelete = true)
        {
            if (isSoftDelete)
            {
                if (entity is IAuditable)
                {
                    // create an entry
                    var entry = Entry((IAuditable)entity);

                    // no need to delete detached entity
                    var initialState = entry.State;
                    if (initialState == EntityState.Detached)
                    {
                        entry.State = EntityState.Unchanged;
                    }

                    // no need to delete entity that has been added since it's not in the db
                    // modify delete data
                    if (initialState == EntityState.Added)
                    {
                        entry.State = EntityState.Detached;
                    }
                    else
                    {
                        entry.Entity.DeletedAt = _dateTimeService.NowUtc;
                        entry.Entity.DeletedById = _authenticatedUser.UserId;
                        entry.Entity.IsDeleted = true;

                        entry.Property(e => e.DeletedAt).IsModified = true;
                        entry.Property(e => e.DeletedById).IsModified = true;
                        entry.Property(e => e.IsDeleted).IsModified = true;
                    }

                    return entry;
                }
                else
                {
                    // throw exception
                    throw new Exception("Entity not soft deletable");
                }
            }
            else
            {
                return base.Remove(entity);
            }
        }

        public override void RemoveRange(IEnumerable<object> entities)
        {
            RemoveRange(entities, true);
        }

        public void RemoveRange(IEnumerable<object> entities, bool isSoftDelete = true)
        {
            if (isSoftDelete == true)
            {
                // An Added entity does not yet exist in the database. If it is then marked as deleted there is
                // nothing to delete because it was not yet inserted, so just make sure it doesn't get inserted.
                foreach (var entity in entities)
                {
                    if (entity is IAuditable)
                    {
                        // create an entry
                        var entry = Entry((IAuditable)entity);

                        // no need to delete detached entity
                        var initialState = entry.State;
                        if (initialState == EntityState.Detached)
                        {
                            entry.State = EntityState.Unchanged;
                        }

                        // no need to delete entity that has been added since it's not in the db
                        // modify delete data
                        if (initialState == EntityState.Added)
                        {
                            entry.State = EntityState.Detached;
                        }
                        else
                        {
                            entry.Entity.DeletedAt = _dateTimeService.NowUtc;
                            entry.Entity.DeletedById = _authenticatedUser.UserId;
                            entry.Entity.IsDeleted = true;

                            entry.Property(e => e.DeletedAt).IsModified = true;
                            entry.Property(e => e.DeletedById).IsModified = true;
                            entry.Property(e => e.IsDeleted).IsModified = true;
                        }
                    }
                }
            }
            else
            {
                base.RemoveRange(entities);
            }
        }

        public virtual EntityEntry Restore(object entity)
        {
            var result = base.Attach(entity);

            if (entity is IAuditable)
            {
                ((IAuditable)result.Entity).DeletedAt = null;
                ((IAuditable)result.Entity).DeletedById = string.Empty;
                ((IAuditable)result.Entity).IsDeleted = false;

                ((IAuditable)result.Entity).RestoredAt = _dateTimeService.NowUtc;
                ((IAuditable)result.Entity).RestoredById = _authenticatedUser.UserId;
                ((IAuditable)result.Entity).IsRestored = true;

                Entry((IAuditable)entity).Property(e => e.DeletedAt).IsModified = true;
                Entry((IAuditable)entity).Property(e => e.DeletedById).IsModified = true;
                Entry((IAuditable)entity).Property(e => e.IsDeleted).IsModified = true;

                Entry((IAuditable)entity).Property(e => e.RestoredAt).IsModified = true;
                Entry((IAuditable)entity).Property(e => e.RestoredById).IsModified = true;
                Entry((IAuditable)entity).Property(e => e.IsRestored).IsModified = true;
            }

            return result;
        }

        public override EntityEntry<TEntity> Update<TEntity>(TEntity entity) where TEntity : class
        {
            // set some data
            if (entity != null && entity is IAuditable)
            {
                // create an entry
                var entry = Entry((IAuditable)entity);

                entry.Entity.IsModified = true;
                entry.Entity.ModifiedAt = _dateTimeService.NowUtc;
                entry.Entity.ModifiedById = _authenticatedUser.UserId;

                Entry((IAuditable)entity).Property(e => e.IsModified).IsModified = true;
                Entry((IAuditable)entity).Property(e => e.ModifiedAt).IsModified = true;
                Entry((IAuditable)entity).Property(e => e.ModifiedById).IsModified = true;
            }

            return base.Update(entity);
        }

        public override EntityEntry Update(object entity)
        {
            // set some data
            if (entity != null && entity is IAuditable)
            {
                // create an entry
                var entry = Entry((IAuditable)entity);

                entry.Entity.IsModified = true;
                entry.Entity.ModifiedAt = _dateTimeService.NowUtc;
                entry.Entity.ModifiedById = _authenticatedUser.UserId;

                Entry((IAuditable)entity).Property(e => e.IsModified).IsModified = true;
                Entry((IAuditable)entity).Property(e => e.ModifiedAt).IsModified = true;
                Entry((IAuditable)entity).Property(e => e.ModifiedById).IsModified = true;
            }

            return base.Update(entity);
        }

        public override void UpdateRange(IEnumerable<object> entities)
        {
            foreach (var entity in entities)
            {
                // set some data
                if (entity is IAuditable)
                {
                    // create an entry
                    var entry = Entry((IAuditable)entity);

                    entry.Entity.IsModified = true;
                    entry.Entity.ModifiedAt = _dateTimeService.NowUtc;
                    entry.Entity.ModifiedById = _authenticatedUser.UserId;

                    Entry((IAuditable)entity).Property(e => e.IsModified).IsModified = true;
                    Entry((IAuditable)entity).Property(e => e.ModifiedAt).IsModified = true;
                    Entry((IAuditable)entity).Property(e => e.ModifiedById).IsModified = true;
                }
            }

            base.UpdateRange(entities);
        }
    }
}
