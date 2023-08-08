﻿namespace Common.Tests.Filtering
{
    public class BasicFilter
    {
        [FilterBy]
        public int? Id { get; set; }

        [FilterBy]
        public long? Amount { get; set; }

        [FilterBy]
        public string? Name { get; set; }

        [FilterBy("Name", CompareWith.Contains)]
        public string? NameContains { get; set; }

        [FilterBy("Name", CompareWith.NotEquals)]
        public string? NameNot { get; internal set; }


        [FilterBy(CompareWith.Contains)]
        public string? Description { get; set; }

        [FilterBy("Id", CompareWith.In)]
        public IEnumerable<int>? SpeficifIdList { get; set; }

        [FilterBy("CreateDate", CompareWith.GreaterThanOrEqual)]
        public DateTime? DateFromInclusive { get; set; }

        [FilterBy("CreateDate", CompareWith.LessThanOrEqual)]
        public DateTime? DateToInclusive { get; set; }

        [FilterBy("CreateDate", CompareWith.GreaterThan)]
        public DateTime? DateFromExclusive { get; set; }

        [FilterBy("CreateDate", CompareWith.LessThan)]
        public DateTime? DateToExclusive { get; set; }

        [FilterBy]
        public DateTime? CreateDate { get; set; }

        [FilterBy("Amount", CompareWith.NotEquals)]
        public long? AmountNot { get; set; }

        [FilterBy("Name", CompareWith.StartsWith)]
        public string? NameStartsWith { get; set; }

        [FilterBy("Name", CompareWith.EndsWith)]
        public string? NameEndsWith { get; set; }


        [FilterBy("Name", CompareWith.Contains)]
        [FilterBy("Description", CompareWith.Contains)]
        [FilterBy("Amount", CompareWith.Equals, stringTransformer: StringTransformer.Int64)]
        public string? Keyword { get; set; }

        [FilterRelation("TestItems", RelationType.InList)]
        public ListFilter? ListFilter { get; set; }

        [FilterRelation("TestItems", RelationType.NotInList)]
        public ListFilter? ListFilterNot { get; set; }

        [FilterRelation("Detail", RelationType.InClass)]
        public DetailsFilter? DetailHas { get; set; }

        [FilterRelation("Detail", RelationType.NotInClass)]
        public DetailsFilter? DetailHasNot { get; set; }
    }

    public class ListFilter
    {
        [FilterBy(CompareWith.Contains)]
        public string? Description { get; set; }

        [FilterBy]
        public long? Amount { get; set; }
    }

    public class DetailsFilter
    {
        [FilterBy(CompareWith.Contains)]
        public string? Description { get; set; }
    }
}
