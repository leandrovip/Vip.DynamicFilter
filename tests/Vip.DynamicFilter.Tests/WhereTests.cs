using Vip.DynamicFilter.Tests.Core;
using Xunit;

namespace Vip.DynamicFilter.Tests;

public class WhereTests
{
    [Fact]
    public void WhereTests_ApplyFilterRequest_WhenSingleFilterWithSingleInputIsPassed_ShouldReturnFilteredQueryable()
    {
        // Arrange
        var request = new FilterRequest
        {
            Where = new Filter
            {
                Column = "Name",
                ConditionType = WhereCondition.NotEqual,
                Value = "Jose da Silva"
            }
        };

        // Act
        var filteredResult = Mock.Clients.ApplyFilterRequest(request);
        var normalResult = Mock.Clients.Where(x => string.CompareOrdinal(x.Name, "Jose da Silva") != 0);

        // Assert
        Helper.EnumarableAreEqual(filteredResult, normalResult);
    }

    [Fact]
    public void WhereTests_ApplyFilterRequest_WhenListOfFilterInputsWithSingleValueArePassed_ShouldReturnFilteredQueryable()
    {
        // Arrange
        var request = new FilterRequest
        {
            Where = new Filter
            {
                OperatorType = Operator.And,
                Filters = new List<Filter>
                {
                    new()
                    {
                        Column = "Age",
                        ConditionType = WhereCondition.GreaterThanOrEqual,
                        Value = 14
                    },
                    new()
                    {
                        Column = "Age",
                        ConditionType = WhereCondition.LessThanOrEqual,
                        Value = 25
                    }
                }
            }
        };

        // Act
        var filteredResult = Mock.Clients.ApplyFilterRequest(request);
        var normalResult = Mock.Clients.Where(p => p.Age >= 14 && p.Age <= 25);

        // Assert
        Helper.EnumarableAreEqual(filteredResult, normalResult);
    }

    [Fact]
    public void WhereTests_ApplyFilterRequest_WhenListOfFilterInputsWithNestedValueArePassed_ShouldReturnFilteredQueryable()
    {
        // Arrange
        var request = new FilterRequest
        {
            Where = new Filter
            {
                OperatorType = Operator.And,
                Filters = new List<Filter>
                {
                    new()
                    {
                        Column = "Age",
                        ConditionType = WhereCondition.GreaterThanOrEqual,
                        Value = 5
                    },
                    new()
                    {
                        Column = "Address.Street",
                        ConditionType = WhereCondition.Contains,
                        Value = "Rua das Palmeiras"
                    }
                }
            }
        };

        // Act
        var filteredResult = Mock.Clients.ApplyFilterRequest(request);
        var normalResult = Mock.Clients.Where(p => p.Age >= 5 && p.Address.Any(a => a.Street.Contains("Rua das Palmeiras")));

        // Assert
        Helper.EnumarableAreEqual(filteredResult, normalResult);
        Assert.NotEmpty(filteredResult);
    }

    [Fact]
    public void WhereTests_ApplyFilterRequest_WhenListOfFilterInputsWithNestedValueArePassedContainsInsensitive_ShouldReturnFilteredQueryable()
    {
        // Arrange
        var request = new FilterRequest
        {
            Where = new Filter
            {
                OperatorType = Operator.And,
                Filters = new List<Filter>
                {
                    new()
                    {
                        Column = "Age",
                        ConditionType = WhereCondition.GreaterThanOrEqual,
                        Value = 5
                    },
                    new()
                    {
                        Column = "Address.Street",
                        ConditionType = WhereCondition.ContainsIgnoreCase,
                        Value = "rua das palmeiras"
                    }
                }
            }
        };

        // Act
        var filteredResult = Mock.Clients.ApplyFilterRequest(request);
        var normalResult = Mock.Clients.Where(p => p.Age >= 5 && p.Address.Any(a => a.Street.Contains("rua das palmeiras", StringComparison.InvariantCultureIgnoreCase)));

        // Assert
        Helper.EnumarableAreEqual(filteredResult, normalResult);
        Assert.NotEmpty(filteredResult);
    }
}