using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SytsBackendGen2.Domain.Common;
using SytsBackendGen2.Domain.Entities;

namespace SytsBackendGen2.Infrastructure.Interceptors
{
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private readonly TimeProvider _dateTime;

        public AuditableEntityInterceptor(
            TimeProvider dateTime)
        {
            _dateTime = dateTime;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateEntities(eventData.Context);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public void UpdateEntities(DbContext? context)
        {
            if (context == null) return;

#if DEBUG
            var entries = context.ChangeTracker.Entries<BaseEntity>();
#endif

            foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.Created = _dateTime.GetUtcNow();
                }

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
                {
                    entry.Entity.LastModified = _dateTime.GetUtcNow();
                }

                if (entry.State == EntityState.Deleted && entry.Entity is INonDelitableEntity entity)
                {
                    entity.Deleted = true;
                    entry.State = EntityState.Unchanged;
                    context.Entry(entity).Property(u => u.Deleted).IsModified = true;
                }
            }
        }
    }

    public static class Extensions
    {
        public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
            entry.References.Any(r =>
                r.TargetEntry != null &&
                r.TargetEntry.Metadata.IsOwned() &&
                (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
    }

}
