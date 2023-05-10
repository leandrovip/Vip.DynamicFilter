using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Vip.DynamicFilter
{
    internal static class WhereExpression
    {
        #region Types

        private static readonly Type _stringType = typeof(string);
        private static readonly Type _expressionType = typeof(Expression);
        private static readonly Type _queryableType = typeof(Queryable);

        #endregion

        #region Available Types Collection

        private static readonly Type[] AvailableCastTypes =
        {
            typeof(DateTime),
            typeof(DateTime?),
            typeof(DateTimeOffset),
            typeof(DateTimeOffset?),
            typeof(TimeSpan),
            typeof(TimeSpan?),
            typeof(bool),
            typeof(bool?),
            typeof(byte?),
            typeof(sbyte?),
            typeof(short),
            typeof(short?),
            typeof(ushort),
            typeof(ushort?),
            typeof(int),
            typeof(int?),
            typeof(uint),
            typeof(uint?),
            typeof(long),
            typeof(long?),
            typeof(ulong),
            typeof(ulong?),
            typeof(Guid),
            typeof(Guid?),
            typeof(double),
            typeof(double?),
            typeof(float),
            typeof(float?),
            typeof(decimal),
            typeof(decimal?),
            typeof(char),
            typeof(char?),
            typeof(string)
        };

        #endregion

        #region Runtime Methods

        private static readonly MethodInfo _andAlsoMethod = _expressionType.GetRuntimeMethod("AndAlso", new[] {_expressionType, _expressionType});
        private static readonly MethodInfo _orElseMethod = _expressionType.GetRuntimeMethod("OrElse", new[] {_expressionType, _expressionType});
        private static readonly MethodInfo _startsWithMethod = _stringType.GetRuntimeMethod("StartsWith", new[] {_stringType});
        private static readonly MethodInfo _containsMethod = _stringType.GetRuntimeMethod("Contains", new[] {_stringType});
        private static readonly MethodInfo _endsWithMethod = _stringType.GetRuntimeMethod("EndsWith", new[] {_stringType});
        private static readonly MethodInfo _asQueryableMethod = _queryableType.GetRuntimeMethods().FirstOrDefault(method => method.Name.Equals("AsQueryable") && method.IsStatic);
        private static readonly MethodInfo _collectionAnyMethod = _queryableType.GetRuntimeMethods().Single(method => method.Name.Equals("Any") && method.IsStatic && method.GetParameters().Length == 1);
        private static readonly MethodInfo _collectionAny2Method = typeof(Queryable).GetRuntimeMethods().Single(method => method.Name.Equals("Any") && method.IsStatic && method.GetParameters().Length == 2);
        private static readonly MethodInfo _expressionMethod = typeof(WhereExpression).GetRuntimeMethods().FirstOrDefault(m => m.Name == "GetExpression");

        #endregion

        #region Extensions

        public static Expression<Func<T, bool>> GetExpression<T>(this Where filter, string suffix = "")
        {
            var e = Expression.Parameter(typeof(T), "e" + suffix);
            var exs = GetExpressionForColumn(e, filter, suffix + "0");
            return Expression.Lambda<Func<T, bool>>(exs, e);
        }

        public static Expression<Func<T, bool>> GetFilterExpression<T>(this Filter filter, string suffix = "")
        {
            var e = Expression.Parameter(typeof(T), "e" + suffix);
            return Expression.Lambda<Func<T, bool>>(GetExpressionForFilterField(e, filter, suffix), e);
        }

        #endregion

        #region Private Methods

        private static Expression GetExpressionForFilterField(Expression e, Filter filter, string suffix)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (filter.OperatorType == Operator.None) return GetExpressionForColumn(e, filter, suffix + "0");
            if (!(filter.Filters?.Any() ?? false)) throw new ArgumentException("Filters with operator type different from Operator.None cannot be empty.");

            var i = 0;
            var expression = GetExpressionForFilterField(e, filter.Filters[i], suffix + i);
            var methodInfo = filter.OperatorType == Operator.And ? _andAlsoMethod : _orElseMethod;
            for (i = 1; i < filter.Filters.Count; i++)
            {
                var args = new object[] {expression, GetExpressionForFilterField(e, filter.Filters[i], suffix + i)};
                expression = (BinaryExpression) methodInfo.Invoke(null, args);
            }

            return expression;
        }

        private static Expression GetExpressionForColumn(Expression e, Where filter, string suffix)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            if (filter.ConditionType == WhereCondition.None || string.IsNullOrWhiteSpace(filter.Column)) throw new ArgumentException("Filter type cannot be None for single filter.");

            var columnArray = filter.Column.Split('.');
            var prop = e;

            foreach (var partColumn in columnArray)
            {
                if (prop.IsEnumerable())
                {
                    prop = AsQueryable(prop);
                    var generic = _expressionMethod.MakeGenericMethod(prop.Type.GenericTypeArguments.Single());
                    object[] pars = {Where.New(partColumn, filter.ConditionType, filter.Value), suffix};
                    var expr = (Expression) generic.Invoke(null, pars);

                    return Expression.Call(_collectionAny2Method.MakeGenericMethod(prop.Type.GenericTypeArguments.First()), prop, expr);
                }

                prop = Expression.Property(prop, GetDeclaringProperty(prop, partColumn));
            }

            var expression = GenerateExpressionByCondition(prop, filter);
            return expression;
        }

        private static Expression GenerateExpressionByCondition(Expression prop, Where filter)
        {
            switch (filter.ConditionType)
            {
                case WhereCondition.Equal:
                    return Expression.Equal(prop, ToStaticParameterExpressionOfType(TryCastColumnValueType(filter.Value, prop.Type), prop.Type));

                case WhereCondition.NotEqual:
                    return Expression.NotEqual(prop, ToStaticParameterExpressionOfType(TryCastColumnValueType(filter.Value, prop.Type), prop.Type));

                case WhereCondition.LessThan:
                    return Expression.LessThan(prop, ToStaticParameterExpressionOfType(TryCastColumnValueType(filter.Value, prop.Type), prop.Type));

                case WhereCondition.GreaterThan:
                    return Expression.GreaterThan(prop, ToStaticParameterExpressionOfType(TryCastColumnValueType(filter.Value, prop.Type), prop.Type));

                case WhereCondition.LessThanOrEqual:
                    return Expression.LessThanOrEqual(prop, ToStaticParameterExpressionOfType(TryCastColumnValueType(filter.Value, prop.Type), prop.Type));

                case WhereCondition.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(prop, ToStaticParameterExpressionOfType(TryCastColumnValueType(filter.Value, prop.Type), prop.Type));

                case WhereCondition.StartsWith:
                    return Expression.Call(prop, _startsWithMethod, Expression.Constant(filter.Value.ToString(), _stringType));

                case WhereCondition.NotStartsWith:
                    return Expression.Not(Expression.Call(prop, _startsWithMethod, Expression.Constant(filter.Value.ToString(), _stringType)));

                case WhereCondition.Contains:
                    return Expression.Call(prop, _containsMethod, Expression.Constant(filter.Value.ToString(), _stringType));

                case WhereCondition.ContainsIgnoreCase:
                    return Expression.Call(prop, "Contains", null, Expression.Constant(filter.Value.ToString(), _stringType), Expression.Constant(StringComparison.InvariantCultureIgnoreCase));

                case WhereCondition.NotContains:
                    return Expression.Not(Expression.Call(prop, _containsMethod, Expression.Constant(filter.Value.ToString(), _stringType)));

                case WhereCondition.EndsWith:
                    return Expression.Call(prop, _endsWithMethod, Expression.Constant(filter.Value.ToString(), _stringType));

                case WhereCondition.NotEndsWith:
                    return Expression.Not(Expression.Call(prop, _endsWithMethod, Expression.Constant(filter.Value.ToString(), _stringType)));

                case WhereCondition.Any:
                    if (prop.IsEnumerable()) prop = AsQueryable(prop);
                    var ca = _collectionAnyMethod.MakeGenericMethod(prop.Type.GenericTypeArguments.First());
                    return Expression.Call(ca, prop);

                case WhereCondition.NotAny:
                    if (prop.IsEnumerable()) prop = AsQueryable(prop);
                    var cna = _collectionAnyMethod.MakeGenericMethod(prop.Type.GenericTypeArguments.First());
                    return Expression.Not(Expression.Call(cna, prop));

                case WhereCondition.IsNull:
                    return Expression.Equal(prop, ToStaticParameterExpressionOfType(null, prop.Type));

                case WhereCondition.IsNotNull:
                    return Expression.Not(Expression.Equal(prop, ToStaticParameterExpressionOfType(null, prop.Type)));

                case WhereCondition.IsEmpty:
                    if (prop.Type != typeof(string)) throw new InvalidCastException($"{filter.ConditionType} can be applied to String type only");
                    return Expression.Equal(prop, ToStaticParameterExpressionOfType(string.Empty, prop.Type));

                case WhereCondition.IsNotEmpty:
                    if (prop.Type != typeof(string)) throw new InvalidCastException($"{filter.ConditionType} can be applied to String type only");
                    return Expression.Not(Expression.Equal(prop, ToStaticParameterExpressionOfType(string.Empty, prop.Type)));

                case WhereCondition.IsNullOrEmpty:
                    if (prop.Type != typeof(string)) throw new InvalidCastException($"{filter.ConditionType} can be applied to String type only");
                    return Expression.OrElse(
                        Expression.Equal(prop, ToStaticParameterExpressionOfType(null, prop.Type)),
                        Expression.Equal(prop, ToStaticParameterExpressionOfType(string.Empty, prop.Type)));

                case WhereCondition.IsNotNullOrEmpty:
                    if (prop.Type != typeof(string)) throw new InvalidCastException($"{filter.ConditionType} can be applied to String type only");
                    return Expression.Not(
                        Expression.OrElse(
                            Expression.Equal(prop, ToStaticParameterExpressionOfType(null, prop.Type)),
                            Expression.Equal(prop, ToStaticParameterExpressionOfType(string.Empty, prop.Type))));
                default:
                    return prop;
            }
        }

        private static Expression ToStaticParameterExpressionOfType(object obj, Type type)
        {
            return Expression.Convert(Expression.Property(Expression.Constant(new {obj}), "obj"), type);
        }

        private static Expression AsQueryable(Expression prop)
        {
            return Expression.Call(_asQueryableMethod.MakeGenericMethod(prop.Type.GenericTypeArguments.Single()), prop);
        }

        private static object TryCastColumnValueType(object value, Type type)
        {
            if (value == null || (!AvailableCastTypes.Contains(type) && !type.GetTypeInfo().IsEnum))
                throw new InvalidCastException($"Cannot convert value to type {type.Name}.");

            var valueType = value.GetType();
            if (valueType == type) return value;

            if (type == typeof(string)) return Convert.ToString(value);
            if (type.GetTypeInfo().BaseType == typeof(Enum)) return Enum.Parse(type, Convert.ToString(value));

            var s = Convert.ToString(value);
            object res;

            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GenericTypeArguments[0];
                res = Activator.CreateInstance(typeof(Nullable<>).MakeGenericType(type));
            }
            else
            {
                res = Activator.CreateInstance(type);
            }

            var argTypes = new[] {_stringType, type.MakeByRefType()};
            object[] args = {s, res};
            var tryParse = type.GetRuntimeMethod("TryParse", argTypes);

            if (!(bool) (tryParse?.Invoke(null, args) ?? false))
                throw new InvalidCastException($"Cannot convert value to type {type.Name}.");

            return args[1];
        }

        private static bool IsEnumerable(this Expression prop)
        {
            return prop.Type.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(x => x.Name == "IEnumerable") != null;
        }

        private static PropertyInfo GetDeclaringProperty(Expression e, string name)
        {
            var type = e.Type;
            var p = type.GetRuntimeProperties().SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (p != null)
            {
                if (type != p.DeclaringType) p = p.DeclaringType.GetRuntimeProperties().SingleOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                return p;
            }

            throw new InvalidOperationException($"Property '{name}' not found on type '{type}'");
        }

        #endregion
    }
}