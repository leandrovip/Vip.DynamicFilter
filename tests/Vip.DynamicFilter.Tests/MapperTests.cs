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
}