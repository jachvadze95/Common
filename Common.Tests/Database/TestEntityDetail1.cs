using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [MaxLength(100)]
        public string Description { get; set; }
        public string UniqueField { get; set; }
    }
}
