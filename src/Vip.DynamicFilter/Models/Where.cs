namespace Vip.DynamicFilter
{
    public class Where
    {
        #region Fields

        private string _condition;

        #endregion

        #region Constructors

        public Where()
        {
            ConditionType = WhereCondition.None;
        }

        #endregion

        #region Properties

        public string Column { get; set; }
        public WhereCondition ConditionType { get; set; }
        public object Value { get; set; }

        public string Condition
        {
            get => _condition;
            set
            {
                ConditionType = ConverterCondition(value);
                _condition = value;
            }
        }

        #endregion

        #region Methods

        private WhereCondition ConverterCondition(string conditionStr)
        {
            if (string.IsNullOrEmpty(conditionStr)) return WhereCondition.None;
            return conditionStr switch
            {
                "none" => WhereCondition.None,
                "=" => WhereCondition.Equal,
                "!=" => WhereCondition.NotEqual,
                "<" => WhereCondition.LessThan,
                ">" => WhereCondition.GreaterThan,
                "<=" => WhereCondition.LessThanOrEqual,
                ">=" => WhereCondition.GreaterThanOrEqual,
                "~" => WhereCondition.Contains,
                "~~" => WhereCondition.ContainsIgnoreCase,
                "!~" => WhereCondition.NotContains,
                "*~" => WhereCondition.StartsWith,
                "!*~" => WhereCondition.NotStartsWith,
                "~*" => WhereCondition.EndsWith,
                "!~*" => WhereCondition.NotEndsWith,
                "any" => WhereCondition.Any,
                "!any" => WhereCondition.NotAny,
                "isnull" => WhereCondition.IsNull,
                "notnull" => WhereCondition.IsNotNull,
                "isempty" => WhereCondition.IsEmpty,
                "notempty" => WhereCondition.IsNotEmpty,
                "isnullorempty" => WhereCondition.IsNullOrEmpty,
                "notnullorempty" => WhereCondition.IsNotNullOrEmpty,
                _ => WhereCondition.None
            };
        }

        #endregion

        #region Static Method

        public static Where New(string column, WhereCondition condition, object value) => new() {Column = column, ConditionType = condition, Value = value};

        #endregion
    }
}