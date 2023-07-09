using Common.Tests.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console
{
    public static class Helpers
    {
        public static IEnumerable<TestEntityItem> GetItems(int outerIndex, int count)
        {
            for(int i = 0; i < count; i++)
            {
                yield return new TestEntityItem
                {
                    Amount = outerIndex * 100 + i,
                    CreateDate = DateTime.Now,
                    Description = $"Description {outerIndex}-{i}",
                    Name = $"Name {outerIndex}-{i}"
                };
            }
        } 

    }
}
