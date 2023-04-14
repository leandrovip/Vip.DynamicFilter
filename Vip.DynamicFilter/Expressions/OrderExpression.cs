using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Vip.DynamicFilter
{
    internal static class OrderExpression
    {
        #region Types

        private static readonly Type _queryableType = typeof(Queryable);

        #endregion

        #region Available Types Collection

        #endregion

        #region Runtime Methods

        private static readonly MethodInfo QueryableOrderBy = _queryableType.GetRuntimeMethods().Single(
            method => method.Name == "OrderBy"
                      && method.IsGenericMethodDefinition
                      && method.GetGenericArguments().Length == 2
                      && method.GetParameters().Length == 2);

        private static readonly MethodInfo QueryableOrderByDescending = _queryableType.GetRuntimeMethods().Single(
            method => method.Name == "OrderByDescending"
                      && method.IsGenericMethodDefinition
                      && method.GetGenericArguments().Length == 2
                      && method.GetParameters().Length == 2);

        private static readonly MethodInfo QueryableThenBy = _queryableType.GetRuntimeMethods().Single(
            method => method.Name == "ThenBy"
                      && method.IsGenericMethodDefinition
                      && method.GetGenericArguments().Length == 2
                      && method.GetParameters().Length == 2);

        private static readonly MethodInfo QueryableThenByDescending = _queryableType.GetRuntimeMethods().Single(
            method => method.Name == "ThenByDescending"
                      && method.IsGenericMethodDefinition
                      && method.GetGenericArguments().Length == 2
                      && method.GetParameters().Length == 2);

        #endregion

        #region Extensions

        public static IOrderedQueryable<T> GetOrderedQueryable<T>(this Order sort, IQueryable<T> data, OrderStep step)
        {
            if (sort == null) throw new ArgumentNullException(nameof(sort));
            var type = typeof(T);

            var arg = Expression.Parameter(type, "x" + step);
            Expression expr = arg;
            var splitColum = sort.Column.Split('.');
            foreach (var part in splitColum)
            {
                var propertyInfo = GetDeclaringProperty(type, part);
                expr = Expression.Property(expr, propertyInfo);
                type = propertyInfo.PropertyType;
            }

            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            Expression lambda = Expression.Lambda(delegateType, expr, arg);
            var m = step == OrderStep.First
                ? sort.Direction == OrderDirection.Asc ? QueryableOrderBy : QueryableOrderByDescending
                : sort.Direction == OrderDirection.Asc
                    ? QueryableThenBy
                    : QueryableThenByDescending;

            return (IOrderedQueryable<T>) m.MakeGenericMethod(typeof(T), type)
                .Invoke(null, new object[] {data, lambda});
        }

        #endregion

        #region Methods

        private static PropertyInfo GetDeclaringProperty(Type t, string name)
        {
            var p = t.GetRuntimeProperties().SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (p != null)
            {
                if (t != p.DeclaringType)
                    p = p.DeclaringType
                        .GetRuntimeProperties()
                        .SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                return p;
            }

            throw new InvalidOperationException($"Property '{name}' not found on type '{t}'");
        }

        #endregion
    }
}