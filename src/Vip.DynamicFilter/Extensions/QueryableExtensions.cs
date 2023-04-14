using System.Collections.Generic;
using System.Linq;

namespace Vip.DynamicFilter
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> RequestBuild<T>(this IQueryable<T> query, FilterRequest request)
        {
            IQueryable<T> res = query.Where(request.Where).OrderBy(request.OrderBy);
            if (request.PageNumber >= 0) res = res.Skip(request.PageNumber * request.Limit);
            if (request.Limit > 0) res = res.Take(request.Limit);

            return res;
        }

        public static IQueryable<T> RequestBuild<T>(this IEnumerable<T> query, FilterRequest request)
        {
            return query.AsQueryable().RequestBuild(request);
        }

        public static IQueryable<T> Where<T>(this IQueryable<T> query, Where filter)
        {
            return filter != null ? query.Where(filter.GetExpression<T>()) : query;
        }

        public static IQueryable<T> Where<T>(this IEnumerable<T> query, Where filter)
        {
            return filter != null ? query.AsQueryable().Where(filter.GetExpression<T>()) : query.AsQueryable();
        }

        public static IQueryable<T> Where<T>(this IQueryable<T> query, Filter filter)
        {
            return filter != null ? query.Where(filter.GetFilterExpression<T>()) : query;
        }

        public static IQueryable<T> Where<T>(this IEnumerable<T> query, Filter filter)
        {
            return filter != null ? query.AsQueryable().Where(filter.GetFilterExpression<T>()) : query.AsQueryable();
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, Order sort)
        {
            return sort != null ? sort.GetOrderedQueryable(query, OrderStep.First) : (IOrderedQueryable<T>) query;
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IEnumerable<T> query, Order order)
        {
            return order != null ? order.GetOrderedQueryable(query.AsQueryable(), OrderStep.First) : (IOrderedQueryable<T>) query.AsQueryable();
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IQueryable<T> query, Order order)
        {
            return order != null ? order.GetOrderedQueryable(query, OrderStep.Next) : (IOrderedQueryable<T>) query;
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IEnumerable<T> query, Order order)
        {
            return order != null ? order.GetOrderedQueryable(query.AsQueryable(), OrderStep.Next) : (IOrderedQueryable<T>) query.AsQueryable();
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> query, IEnumerable<Order> orders)
        {
            var res = (IOrderedQueryable<T>) query;
            var step = OrderStep.First;
            if (orders != null)
                foreach (var filter in orders)
                {
                    res = filter.GetOrderedQueryable(res, step);
                    step = OrderStep.Next;
                }

            return res;
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IEnumerable<T> query, IEnumerable<Order> orders)
        {
            return query.AsQueryable().OrderBy(orders);
        }
    }
}