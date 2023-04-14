using System.Collections.Generic;

namespace Vip.DynamicFilter
{
    public class FilterRequest
    {
        #region Properties

        public Filter Where { get; set; }

        public List<Order> OrderBy { get; set; }

        public int PageNumber { get; set; }

        public int Limit { get; set; }

        #endregion

        #region Constructors

        public FilterRequest()
        {
            Limit = -1;
        }

        #endregion
    }
}