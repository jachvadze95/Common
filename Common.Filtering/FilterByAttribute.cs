namespace Common.Filtering
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FilterByAttribute : Attribute
    {
        public string? ColumnName { get; }
        public CompareWith ComparisonType { get; }
        public LogicalOperator CombineWith { get; set; }
        public StringTransformer StringTransformer { get; set; }

        public FilterByAttribute(string columnName, CompareWith comparisonType, LogicalOperator combineWith = LogicalOperator.Or, StringTransformer stringTransformer = StringTransformer.None)
        {
            ColumnName = columnName;
            ComparisonType = comparisonType;
            CombineWith = combineWith;
            StringTransformer = stringTransformer;
        }

        public FilterByAttribute(CompareWith comparisonType, LogicalOperator combineWith = LogicalOperator.Or, StringTransformer stringTransformer = StringTransformer.None)
        {
            ComparisonType = comparisonType;
            CombineWith = combineWith;
            StringTransformer= stringTransformer;
        }

        public FilterByAttribute(string columnName, LogicalOperator combineWith = LogicalOperator.Or, StringTransformer stringTransformer = StringTransformer.None)   
        {
            ColumnName = columnName;
            ComparisonType = CompareWith.Equals;
            CombineWith = combineWith;
            StringTransformer = stringTransformer;
        }

        public FilterByAttribute(LogicalOperator combineWith = LogicalOperator.Or, StringTransformer stringTransformer = StringTransformer.None)
        {
            ComparisonType = CompareWith.Equals;
            CombineWith = combineWith;
            StringTransformer = stringTransformer;
        }
    }
}
