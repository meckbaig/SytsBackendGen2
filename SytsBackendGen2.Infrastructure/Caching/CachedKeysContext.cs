using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SytsBackendGen2.Infrastructure.Caching;

internal interface ICachedKeysContext
{
    DbSet<CachedKey> CachedKeys { get; }
    DbSet<TypeIdPair> TypeIdPairs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}

internal class CachedKeysContext : DbContext, ICachedKeysContext
{
    public CachedKeysContext(DbContextOptions<CachedKeysContext> options) : base(options)
    {
    }

    public DbSet<CachedKey> CachedKeys
        => Set<CachedKey>();

    public DbSet<TypeIdPair> TypeIdPairs
        => Set<TypeIdPair>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TypeIdPair>()
            .HasKey(t => new { t.CachedKeyId, t.Type, t.EntityId });

        //modelBuilder.Entity<CachedKey>()
        //    .HasMany(c => c.TypeIdPairs)
        //    .WithOne(t => t.CachedKey)
        //    .HasForeignKey(t => t.CachedKeyId);

        modelBuilder.Entity<TypeIdPair>()
            .HasOne(t => t.CachedKey)
            .WithMany(c => c.TypeIdPairs);


        base.OnModelCreating(modelBuilder);
    }
}

[Domain.Common.NotCached]
internal class CachedKey
{
    public int Id { get; set; }
    public string Key { get; set; }
    public DateTimeOffset Expires { get; set; }
    public HashSet<TypeIdPair> TypeIdPairs { get; set; } = [];
    public bool FormationCompleted { get; set; } = false;

    public CachedKey() { }

    public CachedKey(string key, DateTimeOffset expires)
    {
        Key = key;
        Expires = expires;
    }
}

[Domain.Common.NotCached]
internal class TypeIdPair
{
    public int EntityId { get; set; }
    public string Type { get; set; }
    [ForeignKey(nameof(CachedKey))]
    public int CachedKeyId { get; set; }
    public CachedKey CachedKey { get; set; }

    public TypeIdPair() { }

    public TypeIdPair(int entityId, string type, int cachedKeyId = 0)
    {
        EntityId = entityId;
        Type = type;
        CachedKeyId = cachedKeyId;
    }
}
