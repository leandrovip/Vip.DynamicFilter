using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Vip.DynamicFilter.Tests;

public class MapperTests
{
    [Fact]
    public void MapperTests_ShouldMapJsonConditionPassed()
    {
        // Arrange
        const string json = "{\"pageNumber\":2, \"limit\":2, \"where\": {\"column\": \"age\",\"condition\": \"!=\",\"value\": \"Jose da Silva\"}}";

        // Act
        var opt = new JsonSerializerOptions
        {
            Converters = {new JsonStringEnumConverter()},
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var arquivo = JsonSerializer.Deserialize<FilterRequest>(json, opt);

        // Assert
        Assert.Equal(WhereCondition.NotEqual, arquivo.Where.ConditionType);
    }

    [Theory]
    [InlineData(">=", WhereCondition.GreaterThanOrEqual)]
    [InlineData(" >= ", WhereCondition.GreaterThanOrEqual)]
    [InlineData("GreaterThanOrEqual", WhereCondition.GreaterThanOrEqual)]
    [InlineData("greaterthanorequal", WhereCondition.GreaterThanOrEqual)]
    [InlineData("LessThanOrEqual", WhereCondition.LessThanOrEqual)]
    [InlineData("ISNULL", WhereCondition.IsNull)]
    [InlineData("IsNotNull", WhereCondition.IsNotNull)]
    [InlineData("notnull", WhereCondition.IsNotNull)]
    [InlineData("ContainsIgnoreCase", WhereCondition.ContainsIgnoreCase)]
    [InlineData("none", WhereCondition.None)]
    [InlineData("", WhereCondition.None)]
    public void MapperTests_Condition_ShouldAcceptTokenOrEnumName(string condition, WhereCondition expected)
    {
        // Arrange / Act
        var where = new Where {Condition = condition};

        // Assert
        Assert.Equal(expected, where.ConditionType);
        Assert.Equal(condition, where.Condition);
    }

    [Theory]
    [InlineData("3")]
    [InlineData("Equal,NotEqual")]
    [InlineData("=>")]
    [InlineData("maiorque")]
    public void MapperTests_Condition_WhenUnknown_ShouldKeepNone(string condition)
    {
        // Arrange / Act
        var where = new Where {Condition = condition};

        // Assert
        Assert.Equal(WhereCondition.None, where.ConditionType);
    }

    [Fact]
    public void MapperTests_ShouldMapJsonWithEnumNamesInConditionOperatorAndDirection()
    {
        // Arrange
        const string json = @"{
            ""where"": {
                ""operator"": ""And"",
                ""filters"": [
                    { ""column"": ""Age"", ""condition"": ""GreaterThanOrEqual"", ""value"": ""14"" },
                    { ""column"": ""Age"", ""condition"": ""LessThanOrEqual"", ""value"": ""25"" }
                ]
            },
            ""orderBy"": [ { ""column"": ""Age"", ""direction"": ""Desc"" } ]
        }";

        var opt = new JsonSerializerOptions
        {
            Converters = {new JsonStringEnumConverter()},
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Act
        var request = JsonSerializer.Deserialize<FilterRequest>(json, opt);
        var filteredResult = Core.Mock.Clients.ApplyFilterRequest(request);
        var normalResult = Core.Mock.Clients.Where(x => x.Age >= 14 && x.Age <= 25).OrderByDescending(x => x.Age);

        // Assert
        Assert.Equal(Operator.And, request.Where.OperatorType);
        Assert.Equal(WhereCondition.GreaterThanOrEqual, request.Where.Filters[0].ConditionType);
        Assert.Equal(WhereCondition.LessThanOrEqual, request.Where.Filters[1].ConditionType);
        Assert.Equal(OrderDirection.Desc, request.OrderBy[0].DirectionType);
        Core.Helper.EnumarableAreEqual(filteredResult, normalResult);
        Assert.NotEmpty(filteredResult);
    }
}