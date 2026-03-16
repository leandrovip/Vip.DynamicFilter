using System.Collections.Generic;
using System.Linq;

namespace Vip.DynamicFilter
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyFilterRequest<T>(this IQueryable<T> query, FilterRequest request)
        {
            request = request ?? new FilterRequest();

            var res = query;
            if (request.Where != null) res = res.Where(request.Where);
            if (request.OrderBy != null) res = res.OrderBy(request.OrderBy);
            if (request.PageNumber > 0 && request.Limit > 0) res = res.Skip((request.PageNumber - 1) * request.Limit);
            if (request.Limit > 0) res = res.Take(request.Limit);

            return res;
        }

        public static IQueryable<T> ApplyFilterRequest<T>(this IEnumerable<T> query, FilterRequest request)
        {
            return query.AsQueryable().ApplyFilterRequest(request);
        }

        public static IQueryable<T> Where<T>(this IQueryable<T> query, Where filter)
        {
            return filter != null ? query.Where(filter.GetExpression<T>()) : query;
        }

        public static IQueryable<T> Where<T>(this IEnumerable<T> query, Where filter)
        {
            var queryable = query.AsQueryable();
            return filter != null ? queryable.Where(filter.GetExpression<T>()) : queryable;
        }

        public static IQueryable<T> Where<T>(this IQueryable<T> query, Filter filter)
        {
            return filter != null ? query.Where(filter.GetFilterExpression<T>()) : query;
        }

        public static IQueryable<T> Where<T>(this IEnumerable<T> query, Filter filter)
        {
            var queryable = query.AsQueryable();
            return filter != null ? queryable.Where(filter.GetFilterExpression<T>()) : queryable;
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, Order sort)
        {
            return sort != null ? sort.GetOrderedQueryable(query, OrderStep.First) : query;
        }

        public static IQueryable<T> OrderBy<T>(this IEnumerable<T> query, Order order)
        {
            var queryable = query.AsQueryable();
            return order != null ? order.GetOrderedQueryable(queryable, OrderStep.First) : queryable;
        }

        public static IQueryable<T> ThenBy<T>(this IQueryable<T> query, Order order)
        {
            if (order == null) return query;
            if (query is IOrderedQueryable<T> orderedQuery) return order.GetOrderedQueryable(orderedQuery, OrderStep.Next);

            return order.GetOrderedQueryable(query, OrderStep.First);
        }

        public static IQueryable<T> ThenBy<T>(this IEnumerable<T> query, Order order)
        {
            return query.AsQueryable().ThenBy(order);
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, IEnumerable<Order> orders)
        {
            if (orders == null) return query;

            IOrderedQueryable<T> orderedQuery = null;
            var first = true;

            foreach (var order in orders)
            {
                if (order == null) continue;

                if (first)
                {
                    orderedQuery = order.GetOrderedQueryable(query, OrderStep.First);
                    first = false;
                }
                else
                {
                    orderedQuery = order.GetOrderedQueryable(orderedQuery, OrderStep.Next);
                }
            }

            return orderedQuery ?? query;
        }

        public static IQueryable<T> OrderBy<T>(this IEnumerable<T> query, IEnumerable<Order> orders)
        {
            return query.AsQueryable().OrderBy(orders);
        }
    }
}