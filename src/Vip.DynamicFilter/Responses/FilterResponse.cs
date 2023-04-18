using System.Collections.Generic;

namespace Vip.DynamicFilter;

public class FilterResponse<T>
{
    #region Constructors

    public FilterResponse(IEnumerable<T> data, int totalCount, int pageNumber, int limit)
    {
        Data = data;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        Limit = limit;
    }

    #endregion

    #region Properties

    public IEnumerable<T> Data { get; private set; }
    public int TotalCount { get; private set; }
    public int PageNumber { get; private set; }
    public int Limit { get; private set; }

    #endregion
}