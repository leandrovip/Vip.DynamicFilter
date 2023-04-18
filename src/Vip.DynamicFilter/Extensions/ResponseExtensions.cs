using System.Collections.Generic;
using System.Linq;

namespace Vip.DynamicFilter;

public static class ResponseExtensions
{
    public static FilterResponse<T> GetFilterResponse<T>(this IQueryable<T> query, FilterRequest request) where T : class
    {
        IQueryable<T> result = query
            .Where(request.Where)
            .OrderBy(request.OrderBy);

        var count = result.Count();
        if (request.PageNumber > 0) result = result.Skip((request.PageNumber - 1) * request.Limit);
        if (request.Limit > 0) result = result.Take(request.Limit);

        var response = new FilterResponse<T>(result.ToList(), count, request.PageNumber, request.Limit);
        return response;
    }

    public static FilterResponse<T> GetFilterResponse<T>(this IEnumerable<T> query, FilterRequest request) where T : class
    {
        return query.AsQueryable().GetFilterResponse(request);
    }
}