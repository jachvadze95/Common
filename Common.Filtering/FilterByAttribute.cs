namespace Common.Filtering
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FilterByAttribute : Attribute
    {
        public string? ColumnName { get; }
        public CompareWith ComparisonType { get; }
        public CombineWith CombineWith { get; set; }

        public FilterByAttribute(string columnName, CompareWith comparisonType, CombineWith combineWith = CombineWith.Or)
        {
            ColumnName = columnName;
            ComparisonType = comparisonType;
            CombineWith = combineWith;
        }

        public FilterByAttribute(CompareWith comparisonType, CombineWith combineWith = CombineWith.Or)
        {
            ComparisonType = comparisonType;
            CombineWith = combineWith;
        }

        public FilterByAttribute(string columnName, CombineWith combineWith = CombineWith.Or)
        {
            ColumnName = columnName;
            ComparisonType = CompareWith.Equals;
            CombineWith = combineWith;
        }

        public FilterByAttribute(CombineWith combineWith = CombineWith.Or)
        {
            ComparisonType = CompareWith.Equals;
            CombineWith = combineWith;
        }
    }
}
