namespace Common.Filtering
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FilterByAttribute : Attribute
    {
        public string? ColumnName { get; }
        public CompareWith ComparisonType { get; }
        public CombineWith CombineWith { get; set; }
        public Type? ConvertTo { get; set; }

        public FilterByAttribute(string columnName, CompareWith comparisonType, Type? convertTo = null, CombineWith combineWith = CombineWith.Or)
        {
            ColumnName = columnName;
            ComparisonType = comparisonType;
            CombineWith = combineWith;
            ConvertTo = convertTo;
        }

        public FilterByAttribute(CompareWith comparisonType, Type? convertTo = null, CombineWith combineWith = CombineWith.Or)
        {
            ComparisonType = comparisonType;
            CombineWith = combineWith;
            ConvertTo = convertTo;
        }

        public FilterByAttribute(string columnName, Type? convertTo = null, CombineWith combineWith = CombineWith.Or)   
        {
            ColumnName = columnName;
            ComparisonType = CompareWith.Equals;
            CombineWith = combineWith;
            ConvertTo = convertTo;
        }

        public FilterByAttribute(Type? convertTo = null, CombineWith combineWith = CombineWith.Or)
        {
            ComparisonType = CompareWith.Equals;
            CombineWith = combineWith;
            ConvertTo = convertTo;
        }
    }
}
