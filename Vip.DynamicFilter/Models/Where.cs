namespace Vip.DynamicFilter
{
    public class Where
    {
        #region Constructors

        public Where()
        {
            Condition = WhereCondition.None;
        }

        #endregion

        #region Properties

        public string Column { get; set; }
        public WhereCondition Condition { get; set; }
        public object Value { get; set; }

        #endregion

        #region Static Method

        public static Where New(string column, WhereCondition condition, object value) => new() {Column = column, Condition = condition, Value = value};

        #endregion
    }
}