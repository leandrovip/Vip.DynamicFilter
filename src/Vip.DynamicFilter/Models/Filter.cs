using System.Collections.Generic;

namespace Vip.DynamicFilter
{
    public class Filter : Where
    {
        #region Fields

        private string _operator;
        private Operator _operatorType;
        private bool _operatorUnknown;

        #endregion

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

        public List<Filter> Filters { get; set; }

        public Operator OperatorType
        {
            get => _operatorType;
            set
            {
                _operatorType = value;
                _operatorUnknown = false;
            }
        }

        public string Operator
        {
            get => _operator;
            set
            {
                _operator = value;

                var known = TryConvertOperator(value, out var operatorType);
                OperatorType = operatorType;
                _operatorUnknown = !known;
            }
        }

        /// <summary>
        ///     Indica que <see cref="Operator" /> recebeu um valor diferente de "and", "or" e "none".
        /// </summary>
        internal bool IsOperatorUnknown => _operatorUnknown;

        #endregion

        #region Methods

        private static bool TryConvertOperator(string value, out Operator operatorType)
        {
            operatorType = DynamicFilter.Operator.None;
            if (string.IsNullOrWhiteSpace(value)) return true;

            switch (value.Trim().ToLowerInvariant())
            {
                case "and":
                    operatorType = DynamicFilter.Operator.And;
                    return true;
                case "or":
                    operatorType = DynamicFilter.Operator.Or;
                    return true;
                case "none":
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }
}
