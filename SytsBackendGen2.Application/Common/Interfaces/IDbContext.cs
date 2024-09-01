using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SytsBackendGen2.Application.Common.Interfaces;

public interface IDbContext
{
    DatabaseFacade Database {  get; }
    DbSet<T> Set<T>() where T : class;
    EntityEntry<T> Entry<T> (T entity) where T : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
