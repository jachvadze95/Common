using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tests.Database
{
    public class TestEntityDetail1
    {
        public int Id { get; set; }
        public TestEntity? Parent { get; set; }
        public int? ParentId { get; set; }
        public string Description { get; set; }
        public string UniqueField { get; set; }
    }
}
