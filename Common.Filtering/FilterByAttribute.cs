namespace Common.Filtering
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FilterByAttribute : Attribute
    {
        public string? ColumnName { get; }
        public CompareWith ComparisonType { get; }

        public FilterByAttribute(string columnName, CompareWith comparisonType)
        {
            ColumnName = columnName;
            ComparisonType = comparisonType;
        }

        public FilterByAttribute(CompareWith comparisonType)
        {
            ComparisonType = comparisonType;
        }

        public FilterByAttribute(string columnName)
        {
            ColumnName = columnName;
            ComparisonType = CompareWith.Equals;
        }

        public FilterByAttribute()
        {
            ComparisonType = CompareWith.Equals;
        }
    }
}
