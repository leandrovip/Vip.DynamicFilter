using System.ComponentModel;

namespace Vip.DynamicFilter
{
    public enum WhereCondition
    {
        [Description("none")] None,
        [Description("=")] Equal,
        [Description("!=")] NotEqual,
        [Description("<")] LessThan,
        [Description(">")] GreaterThan,
        [Description("<=")] LessThanOrEqual,
        [Description(">=")] GreaterThanOrEqual,
        [Description("~")] Contains,
        [Description("~~")] ContainsIgnoreCase,
        [Description("!~")] NotContains,
        [Description("*~")] StartsWith,
        [Description("!*~")] NotStartsWith,
        [Description("~*")] EndsWith,
        [Description("!~*")] NotEndsWith,
        [Description("any")] Any,
        [Description("!any")] NotAny,
        [Description("isnull")] IsNull,
        [Description("notnull")] IsNotNull,
        [Description("isempty")] IsEmpty,
        [Description("notempty")] IsNotEmpty,
        [Description("isnullorempty")] IsNullOrEmpty,
        [Description("notnullorempty")] IsNotNullOrEmpty
    }
}