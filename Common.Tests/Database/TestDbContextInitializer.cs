using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tests.Database
{
    public class TestDbContextInitializer
    {
        private readonly TestDbContext _context;
        private readonly ILogger<TestDbContextInitializer> _logger;

        public TestDbContextInitializer(TestDbContext context, ILogger<TestDbContextInitializer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InitialiseAsync()
        {
            try
            {
                //initialize inmemory database
                await _context.Database.EnsureDeletedAsync(); // Drop the existing database
                await _context.Database.EnsureCreatedAsync(); // Create a new in-memory database
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initialising the database.");
                throw;
            }
        }

        public async Task SeedAsync()
        {
            try
            {
                await SeedTestEntities();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private async Task SeedTestEntities()
        {
            var testEntities = new List<TestEntity>();

            for (int i = 1; i <= 10; i++)
            {
                var testEntity = new TestEntity
                {
                    Id = i,
                    Name = $"Test Entity {i}",
                    TestItems = new List<TestEntityItem>()
                };

                for (int j = 1; j <= 10; j++)
                {
                    var testEntityItem = new TestEntityItem
                    {
                        Id = (i - 1) * 10 + j,
                        Amount = j * 100,
                        Name = $"Item {j}",
                        Description = $"Description for Item {j} of Entity {i}",
                        CreateDate = DateTime.Now,
                        TestListId = i
                    };

                    testEntity.TestItems.Add(testEntityItem);
                }

                testEntities.Add(testEntity);
            }

            await _context.TestEntities.AddRangeAsync(testEntities);
            await _context.SaveChangesAsync();
        }
    }
}
