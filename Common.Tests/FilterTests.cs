using Common.Tests.Database;
using Common.Tests.Filtering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Common.Tests
{
    [TestClass]
    public class UnitTest1
    {
        private TestDbContext _dbContext;
        private TestDbContextInitializer _dbContextInitializer;
        private ServiceProvider _sp;

        private TestEntity[] _entities;
        private const string _connectionString = "Data Source=HAWK;Initial Catalog=TestDB;Integrated Security=True;TrustServerCertificate=True";

        public UnitTest1()
        {
            var services = new ServiceCollection();

            services.AddDbContext<TestDbContext>(options =>
                options.UseSqlServer(_connectionString,
                builder => builder.MigrationsAssembly(typeof(TestDbContext).Assembly.FullName)));

            services.AddScoped<TestDbContext>();
            services.AddScoped<TestDbContextInitializer>();


            var serviceProvider = services.BuildServiceProvider();
            _sp = serviceProvider;
            _dbContext = serviceProvider.GetRequiredService<TestDbContext>();
            _dbContextInitializer = serviceProvider.GetRequiredService<TestDbContextInitializer>();

            // Additional setup code if needed 
        }

        [TestInitialize]
        public async Task TestInitialize()
        {
            var text = File.ReadAllText(@"C:\Users\jachv\source\repos\Common\Common.Tests\Database\data.json");
            _entities = JsonConvert.DeserializeObject<List<TestEntity>>(text).ToArray();
            await _dbContextInitializer.InitialiseAsync(_entities);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Clean up resources, if any

            _dbContext.Dispose();
        }

        [TestMethod]
        public async Task FilterWithEquals()
        {
            var randomNum = new Random().Next(0, 10);

            var filter = new BasicFilter()
            {
                Name = randomNum.ToString()
            };

            var dbResults = await _dbContext.TestEntities.AsQueryable().FilterBy(filter).ToListAsync();
            var result = _entities.Where(x => x.Name == filter.Name).ToList();

            Assert.AreEqual(result.Count, dbResults.Count);

            if (result.Count > 0)
                Assert.AreEqual(result[0].Name, dbResults[0].Name);
        }

        [TestMethod]
        public async Task FilterWithNotEquals()
        {
            var name = GetRandomEntity().Name;

            var filter = new BasicFilter()
            {
                NameNot = name
            };

            var dbResults = await _dbContext.TestEntities.AsQueryable().FilterBy(filter).ToListAsync();
            var result = _entities.Where(x => x.Name != filter.NameNot).ToList();

            Assert.AreEqual(result.Count, dbResults.Count);

            if (result.Count > 0)
                Assert.AreEqual(result[0].Name, dbResults[0].Name);
        }

        [TestMethod]
        public async Task FilterWithGreaterThanOrEquals()
        {
            var randomEntity = GetRandomEntity();

            var filter = new BasicFilter()
            {
                DateFromInclusive = randomEntity.CreateDate.AddSeconds(-5),
            };

            var dbResults = await _dbContext.TestEntities.AsQueryable().FilterBy(filter).ToListAsync();
            var result = _entities.Where(x => x.CreateDate >= filter.DateFromInclusive).ToList();

            Assert.AreEqual(result.Count, dbResults.Count);

            if (result.Count > 0)
                Assert.AreEqual(result[0].Name, dbResults[0].Name);
        }

        [TestMethod]
        public async Task FilterWithGreaterThan()
        {
            var randomEntity = GetRandomEntity();

            var filter = new BasicFilter()
            {
                DateFromExclusive = randomEntity.CreateDate.AddMinutes(-5)
            };

            var dbResults = await _dbContext.TestEntities.AsQueryable().FilterBy(filter).ToListAsync();
            var result = _entities.Where(x => x.CreateDate > filter.DateFromExclusive).ToList();

            Assert.AreEqual(result.Count, dbResults.Count);
            Assert.AreEqual(result[0].Name, dbResults[0].Name);
        }

        [TestMethod]
        public async Task FilterWithLessThanOrEquals()
        {
            var randomEntity = GetRandomEntity();

            var filter = new BasicFilter()
            {
                DateToInclusive = randomEntity.CreateDate.AddSeconds(-5),
            };

            var dbResults = await _dbContext.TestEntities.AsQueryable().FilterBy(filter).ToListAsync();
            var result = _entities.Where(x => x.CreateDate <= filter.DateToInclusive).ToList();

            Assert.AreEqual(result.Count, dbResults.Count);

            if (result.Count > 0)
                Assert.AreEqual(result[0].Name, dbResults[0].Name);
        }

        [TestMethod]
        public async Task FilterWithLessThan()
        {
            var randomEntity = GetRandomEntity();

            var filter = new BasicFilter()
            {
                DateToExclusive = randomEntity.CreateDate.AddSeconds(-5),
            };

            var dbResults = await _dbContext.TestEntities.AsQueryable().FilterBy(filter).ToListAsync();
            var result = _entities.Where(x => x.CreateDate < filter.DateToExclusive).ToList();

            Assert.AreEqual(result.Count, dbResults.Count);

            if (result.Count > 0)
                Assert.AreEqual(result[0].Name, dbResults[0].Name);
        }

        [TestMethod]
        public async Task FilterWithContains()
        {
            var randomNum = new Random().Next(0, 10);

            var filter = new BasicFilter()
            {
                NameContains = randomNum.ToString()
            };

            var dbResults = await _dbContext.TestEntities.AsQueryable().FilterBy(filter).ToListAsync();
            var result = _entities.Where(x => x.Name.Contains(filter.NameContains)).ToList();

            Assert.AreEqual(result.Count, dbResults.Count);

            if (result.Count > 0)
                Assert.AreEqual(result[0].Name, dbResults[0].Name);
        }

        [TestMethod]
        public async Task FilterFull()
        {

            var randomNum = new Random().Next(0, 10);

            var filter = new BasicFilter()
            {
                Name = randomNum.ToString(),
                NameNot = randomNum.ToString(),
                DateFromInclusive = DateTime.Now.AddSeconds(-5),
                DateFromExclusive = DateTime.Now.AddSeconds(-5),
                DateToInclusive = DateTime.Now.AddSeconds(-5),
                DateToExclusive = DateTime.Now.AddSeconds(-5),
                NameContains = randomNum.ToString(),
                SpeficifIdList = Enumerable.Range(1, 1000).ToList()
            };

            var dbResults = await _dbContext.TestEntities.AsQueryable().FilterBy(filter).ToListAsync();
            var result = _entities.Where(x => x.Name == filter.Name
                                                       && x.Name != filter.NameNot
                                                                                                  && (x.Description.Contains(filter.Keyword) || x.Name.Contains(filter.Keyword))
                                                                                                                                             && x.CreateDate >= filter.DateFromInclusive
                                                                                                                                                                                        && x.CreateDate > filter.DateFromExclusive
                                                                                                                                                                                                                                   && x.CreateDate <= filter.DateToInclusive
                                                                                                                                                                                                                                                                              && x.CreateDate < filter.DateToExclusive
                                                                                                                                                                                                                                                                                                                         && x.Name.Contains(filter.NameContains)).ToList();

            Assert.AreEqual(result.Count, dbResults.Count);

            if (result.Count > 0)
                Assert.AreEqual(result[0].Name, dbResults[0].Name);

        }

        [TestMethod]
        public async Task FilterListRelation()
        {
            var randomNum = new Random().Next(0, 10);

            var filter = new BasicFilter()
            {
                ListFilter = new ListFilter
                {
                    Amount = 404,
                    Description = "100"
                }
            };

            var dbResults = await _dbContext.TestEntities.AsQueryable().FilterBy(filter, true).ToListAsync();
            var result = _entities.Where(x => x.TestItems.Any(i => i.Description.Contains(filter.ListFilter.Description) && i.Amount == filter.ListFilter.Amount)).ToList();

            Assert.AreEqual(result.Count, dbResults.Count);

            if (result.Count > 0)
                Assert.AreEqual(result[0].Name, dbResults[0].Name);

        }

        [TestMethod]
        public async Task FilterWithMultipleAndConvert()
        {
            var randomNum = new Random().Next();
            var filter = new BasicFilter()
            {
                Keyword = "5"
            };

            decimal.TryParse(filter.Keyword, out decimal keywordAmount);
            var dbResults = await _dbContext.TestEntitiesItems.AsQueryable().FilterBy(filter, true).ToListAsync();
            var result = _entities.SelectMany(x => x.TestItems).Where(x =>
            x.Name.Contains(filter.Keyword) || x.Description.Contains(filter.Keyword) || x.Amount == keywordAmount).ToList();

            Assert.AreEqual(result.Count, dbResults.Count);

            if (result.Count > 0)
                Assert.AreEqual(result[0].Name, dbResults[0].Name);


        }

        [TestMethod]
        public async Task FilterWithInList()
        {
            var randomNum = new Random().Next(0, 10);

            var filter = new BasicFilter()
            {
                ListFilter = new ListFilter
                {
                    Amount = 404,
                    Description = "4"
                }
            };

            var dbResults = await _dbContext.TestEntities.AsQueryable().FilterBy(filter, true).ToListAsync();
            var result = _entities.Where(x => x.TestItems.Any(i => i.Description.Contains(filter.ListFilter.Description) && i.Amount == filter.ListFilter.Amount)).ToList();

            Assert.AreEqual(result.Count, dbResults.Count);

            if (result.Count > 0)
                Assert.AreEqual(result[0].Name, dbResults[0].Name);
        }

        [TestMethod]
        public async Task FilterWithNotInList()
        {
            var randomNum = new Random().Next(0, 10);

            var filter = new BasicFilter()
            {
                ListFilterNot = new ListFilter
                {
                    Amount = 404,
                }
            };

            var dbResults = await _dbContext.TestEntities.AsQueryable().FilterBy(filter, true).ToListAsync();
            var result = _entities.Where(x => !x.TestItems.Any(i => i.Amount == filter.ListFilterNot.Amount)).ToList();

            Assert.AreEqual(result.Count, dbResults.Count);

            if (result.Count > 0)
                Assert.AreEqual(result[0].Name, dbResults[0].Name);
        }

        [TestMethod]
        public async Task FilterInClass()
        {
            var randomNum = new Random().Next(0, 10);

            var filter = new BasicFilter()
            {
                DetailHas = new DetailsFilter
                {
                    Description = "404"
                }
            };

            var dbResults = await _dbContext.TestEntities.AsQueryable().FilterBy(filter, true).ToListAsync();
            var result = _entities.Where(x => x.Detail!.Description.Contains(filter.DetailHas.Description)).ToList();

            Assert.AreEqual(result.Count, dbResults.Count);

            if (result.Count > 0)
                Assert.AreEqual(result[0].Name, dbResults[0].Name);
        }

        [TestMethod]
        public async Task FilterNotInClass()
        {
            var randomNum = new Random().Next(0, 10);

            var filter = new BasicFilter()
            {
                DetailHasNot = new DetailsFilter
                {
                    Description = "404"
                }
            };

            var dbResults = await _dbContext.TestEntities.AsQueryable().FilterBy(filter, true).ToListAsync();
            var result = _entities.Where(x => x.Detail!.Description != filter.DetailHasNot.Description).ToList();

            Assert.AreEqual(result.Count, dbResults.Count);

            if (result.Count > 0)
                Assert.AreEqual(result[0].Name, dbResults[0].Name);
        }

        private TestEntity GetRandomEntity()
        {
            var random = new Random();
            return _entities[random.Next(0, _entities.Count())];
        }
    }
}