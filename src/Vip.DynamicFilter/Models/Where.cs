using System;
using System.Collections.Generic;

namespace Vip.DynamicFilter
{
    public class Where
    {
        #region Fields

        private static readonly Dictionary<string, WhereCondition> _conditions = CreateConditions();

        private string _condition;
        private WhereCondition _conditionType;
        private bool _conditionUnknown;

        #endregion

        #region Constructors

        public Where()
        {
            ConditionType = WhereCondition.None;
        }

        #endregion

        #region Properties

        public string Column { get; set; }
        public object Value { get; set; }

        public WhereCondition ConditionType
        {
            get => _conditionType;
            set
            {
                _conditionType = value;
                _conditionUnknown = false;
            }
        }

        public string Condition
        {
            get => _condition;
            set
            {
                _condition = value;

                var known = TryConvertCondition(value, out var conditionType);
                ConditionType = conditionType;
                _conditionUnknown = !known;
            }
        }

        /// <summary>
        ///     Indica que <see cref="Condition" /> recebeu um valor que não é token nem nome de <see cref="WhereCondition" />.
        /// </summary>
        internal bool IsConditionUnknown => _conditionUnknown;

        #endregion

        #region Methods

        /// <summary>
        ///     Aceita o token (">=") ou o nome do enum ("GreaterThanOrEqual"), sem diferenciar maiúsculas e ignorando espaços nas pontas.
        /// </summary>
        private static bool TryConvertCondition(string conditionStr, out WhereCondition condition)
        {
            condition = WhereCondition.None;
            if (string.IsNullOrWhiteSpace(conditionStr)) return true;

            return _conditions.TryGetValue(conditionStr.Trim(), out condition);
        }

        private static Dictionary<string, WhereCondition> CreateConditions()
        {
            var conditions = new Dictionary<string, WhereCondition>(StringComparer.OrdinalIgnoreCase)
            {
                ["none"] = WhereCondition.None,
                ["="] = WhereCondition.Equal,
                ["!="] = WhereCondition.NotEqual,
                ["<"] = WhereCondition.LessThan,
                [">"] = WhereCondition.GreaterThan,
                ["<="] = WhereCondition.LessThanOrEqual,
                [">="] = WhereCondition.GreaterThanOrEqual,
                ["~"] = WhereCondition.Contains,
                ["~~"] = WhereCondition.ContainsIgnoreCase,
                ["!~"] = WhereCondition.NotContains,
                ["*~"] = WhereCondition.StartsWith,
                ["!*~"] = WhereCondition.NotStartsWith,
                ["~*"] = WhereCondition.EndsWith,
                ["!~*"] = WhereCondition.NotEndsWith,
                ["any"] = WhereCondition.Any,
                ["!any"] = WhereCondition.NotAny,
                ["isnull"] = WhereCondition.IsNull,
                ["notnull"] = WhereCondition.IsNotNull,
                ["isempty"] = WhereCondition.IsEmpty,
                ["notempty"] = WhereCondition.IsNotEmpty,
                ["isnullorempty"] = WhereCondition.IsNullOrEmpty,
                ["notnullorempty"] = WhereCondition.IsNotNullOrEmpty
            };

            // Nomes do enum ("GreaterThanOrEqual", "IsNotNull", ...) também são aceitos.
            foreach (WhereCondition value in Enum.GetValues(typeof(WhereCondition))) conditions[value.ToString()] = value;

            return conditions;
        }

        #endregion

        #region Static Method

        public static Where New(string column, WhereCondition condition, object value) => new() {Column = column, ConditionType = condition, Value = value};

        #endregion
    }
}
