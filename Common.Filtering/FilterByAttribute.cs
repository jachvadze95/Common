namespace Common.Filtering
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FilterByAttribute : Attribute
    {
        public string? ColumnName { get; }
        public CompareWith ComparisonType { get; }
        public CombineWith CombineWith { get; set; }
        public Type? ConvertTo { get; set; }

        public FilterByAttribute(string columnName, CompareWith comparisonType, CombineWith combineWith = CombineWith.Or, Type? convertTo = null)
        {
            ColumnName = columnName;
            ComparisonType = comparisonType;
            CombineWith = combineWith;
            ConvertTo = convertTo;
        }

        public FilterByAttribute(CompareWith comparisonType, CombineWith combineWith = CombineWith.Or, Type? convertTo = null)
        {
            ComparisonType = comparisonType;
            CombineWith = combineWith;
            ConvertTo = convertTo;
        }

        public FilterByAttribute(string columnName, CombineWith combineWith = CombineWith.Or, Type? convertTo = null)   
        {
            ColumnName = columnName;
            ComparisonType = CompareWith.Equals;
            CombineWith = combineWith;
            ConvertTo = convertTo;
        }

        public FilterByAttribute(CombineWith combineWith = CombineWith.Or, Type? convertTo = null)
        {
            ComparisonType = CompareWith.Equals;
            CombineWith = combineWith;
            ConvertTo = convertTo;
        }
    }
}
