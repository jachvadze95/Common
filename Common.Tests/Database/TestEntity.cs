using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tests.Database
{
    public class TestEntity
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; }
        [MaxLength(100)]
        public string Description { get; set; }

        public DateTime CreateDate { get; set; }

        public ICollection<TestEntityItem> TestItems { get; set; }

        public TestEntityDetail1? Detail { get; set; }
    }
}
