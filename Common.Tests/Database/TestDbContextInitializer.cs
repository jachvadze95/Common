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

        public TestDbContextInitializer(TestDbContext context)
        {
            _context = context;
        }

        public async Task InitialiseAsync(List<TestEntity> entities)
        {
            try
            {
                //initialize inmemory database
                await _context.Database.EnsureDeletedAsync(); // Drop the existing database
                await _context.Database.EnsureCreatedAsync(); // Create a new in-memory database

                //Seed the database
                foreach (var entity in entities)
                {
                    var randDifference = new Random().Next(1, 100);

                    entity.CreateDate = DateTime.Now.AddMinutes(randDifference);

                    await _context.TestEntities.AddAsync(entity);

                    foreach (var item in entity.TestItems)
                    {
                        item.CreateDate = DateTime.Now.AddMinutes(randDifference);
                    }
                }
                await _context.TestEntities.AddRangeAsync(entities);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
