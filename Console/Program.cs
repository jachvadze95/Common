using Common.Tests.Database;
using Console;
using Newtonsoft.Json;
using System.Xml;

List<TestEntity> testEntities = new List<TestEntity>();

for (int i = 1; i <= 100; i++)
{
    var randomCount = new Random().Next(1, 20);
    var coinToss = new Random().Next(0, 1) == 1;

    var testEntity = new TestEntity
    {
        Id = i,
        Name = "TestEntity " + i,
        TestItems = Helpers.GetItems(i, randomCount).ToList(),
        Detail1 = coinToss ? new TestEntityDetail1
        {
            Id = i,
            ParentId = i,
            Description = "Detail " + i,
            UniqueField = "UniqueField " + i
        } : null,
        Detail2 = !coinToss ? new TestEntityDetail2
        {
            Id = i,
            ParentId = i,
            Description = "Detail " + i
        } : null
    };

    testEntities.Add(testEntity);
}

string json = JsonConvert.SerializeObject(testEntities);

File.WriteAllText("C:\\Users\\jachv\\source\\repos\\Common\\Common.Tests\\Database\\data.json", json);