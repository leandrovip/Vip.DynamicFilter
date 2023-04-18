using System.Collections.Generic;

namespace Vip.DynamicFilter
{
    public class Filter : Where
    {
        private string _operator;

        #region Constructors

        public Filter()
        {
            Filters = new List<Filter>();
            OperatorType = DynamicFilter.Operator.None;
        }

        public Filter(string column, WhereCondition conditionType, object value) : this()
        {
            Column = column;
            ConditionType = conditionType;
            Value = value;
        }

        #endregion

        #region Properties

        public Operator OperatorType { get; set; }
        public List<Filter> Filters { get; set; }

        public string Operator
        {
            get => _operator;
            set
            {
                _operator = value;
                OperatorType = ConvertOperator(_operator);
            }
        }

        #endregion

        #region Methods

        private Operator ConvertOperator(string value)
        {
            if (string.IsNullOrEmpty(value)) return DynamicFilter.Operator.None;
            value = value.Trim().ToLower();

            return value switch
            {
                "and" => DynamicFilter.Operator.And,
                "or" => DynamicFilter.Operator.Or,
                "none" => DynamicFilter.Operator.None,
                _ => DynamicFilter.Operator.None
            };
        }

        #endregion
    }
}