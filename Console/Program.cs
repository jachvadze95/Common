using Common.Tests.Database;
using Console;
using Newtonsoft.Json;
using System.Xml;

List<TestEntity> testEntities = new List<TestEntity>();

for (int i = 1; i <= 100; i++)
{
    var randomCount = new Random().Next(1, 20);

    var testEntity = new TestEntity
    {
        Name = "TestEntity " + i,
        Description = "TestEntity Details" + i,
        TestItems = Helpers.GetItems(i, randomCount).ToList(),
        CreateDate = DateTime.Now,
        Detail = new TestEntityDetail1
        {
            ParentId = i,
            Description = "Detail " + i,
            UniqueField = "UniqueField " + i
        }
    };

    testEntities.Add(testEntity);
}

string json = JsonConvert.SerializeObject(testEntities);

File.WriteAllText("C:\\Users\\jachv\\source\\repos\\Common\\Common.Tests\\Database\\data.json", json);