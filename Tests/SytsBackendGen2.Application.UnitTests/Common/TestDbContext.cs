using Microsoft.EntityFrameworkCore;
using SytsBackendGen2.Application.Common.Interfaces;
using SytsBackendGen2.Application.UnitTests.Common.Entities;

namespace SytsBackendGen2.Application.UnitTests.Common;

public class TestDbContext : DbContext, IDbContext
{
    public DbSet<TestEntity> TestEntities { get; set; }
    public DbSet<TestNestedEntity> TestNestedEntities { get; set; }
    public DbSet<TestAndNested> TestAndNesteds { get; set; }
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>()
            .HasMany(x => x.TestNestedEntities)
            .WithMany(x => x.TestEntities)
            .UsingEntity<TestAndNested>();
        modelBuilder.Entity<TestEntity>()
            .HasOne(t => t.InnerEntity)
            .WithMany()
            .HasForeignKey(t => t.InnerEntityId);
        base.OnModelCreating(modelBuilder);
    }
}
