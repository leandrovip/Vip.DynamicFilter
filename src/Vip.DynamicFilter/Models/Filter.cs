using System.Collections.Generic;

namespace Vip.DynamicFilter
{
    public class Filter : Where
    {
        #region Constructors

        public Filter(List<Filter> filters)
        {
            Filters = filters;
            Operator = Operator.None;
        }

        #endregion

        #region Properties

        public Operator Operator { get; set; }
        public List<Filter> Filters { get; set; }

        #endregion
    }
}