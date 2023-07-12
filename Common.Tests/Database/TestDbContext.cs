using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tests.Database
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }

        public DbSet<TestEntityItem> TestEntitiesItems { get; set; }

        public DbSet<TestEntityDetail1> TestEntityDetail { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>()
                .HasMany(x => x.TestItems)
                .WithOne(x => x.TestList)
                .HasForeignKey(x => x.TestListId);

            modelBuilder.Entity<TestEntity>()
                .HasOne(x => x.Detail)
                .WithOne(x => x.Parent)
                .HasForeignKey<TestEntityDetail1>(x => x.ParentId);

            base.OnModelCreating(modelBuilder);
        }


    }
}
