using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Tests.Filtering
{
    public class BasicFilter
    {
        [FilterBy]
        public int? Id { get; set; }

        [FilterBy]
        public long? Amount { get; set; }

        [FilterBy]
        public string? Name { get; set; }

        [FilterBy(CompareWith.Contains)]
        public string? Description { get; set; }

        [FilterBy("Id", CompareWith.In)]
        public IEnumerable<int>? SpeficifIdList { get; set; }

        [FilterBy("CreateDate", CompareWith.GreaterThan)]
        public DateTime DateFromInclusive { get; set; }

        [FilterBy("CreateDate", CompareWith.LessThan)]
        public DateTime DateToInclusive { get; set; }

        [FilterBy("CreateDate", CompareWith.GreaterThanOrEqual)]
        public DateTime DateFromExclusive { get; set; }

        [FilterBy("CreateDate", CompareWith.LessThanOrEqual)]
        public DateTime DateToExclusive { get; set; }

        [FilterBy]
        public DateTime CreateDate { get; set; }

        [FilterBy("Amount", CompareWith.NotEquals)]
        public long? AmountNot { get; set; }

        [FilterBy("Name", CompareWith.StartsWith)]
        public string? NameStartsWith { get; set; }

        [FilterBy("Name", CompareWith.EndsWith)]
        public string? NameEndsWith { get; set; }
    }
}
