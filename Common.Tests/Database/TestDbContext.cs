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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>()
                .HasMany(x => x.TestItems)
                .WithOne(x => x.TestList)
                .HasForeignKey(x => x.TestListId);

            base.OnModelCreating(modelBuilder);
        }


    }
}
