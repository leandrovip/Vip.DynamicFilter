using Vip.DynamicFilter.Tests.Core;
using Vip.DynamicFilter.Tests.Models;
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
    public void WhereTests_ApplyFilterRequest_WhenTwoListOfFilterInputsWithNestedValueArePassed_ShouldReturnFilteredQueryable()
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
                        Column = "Name",
                        ConditionType = WhereCondition.Contains,
                        Value = "Silva"
                    },
                    new()
                    {
                        OperatorType = Operator.Or,
                        Filters = new List<Filter>
                        {
                            new()
                            {
                                Column = "Age",
                                ConditionType = WhereCondition.Equal,
                                Value = "22"
                            },
                            new()
                            {
                                Column = "Age",
                                ConditionType = WhereCondition.Equal,
                                Value = "25"
                            }
                        }
                    }
                },
            }
        };

        // Act
        var filteredResult = Mock.Clients.ApplyFilterRequest(request);
        var normalResult = Mock.Clients.Where(p => (p.Name.Contains("Silva")) && (p.Age == 22 || p.Age == 25));

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

    [Fact]
    public void WhereTests_ApplyFilterRequest_WhentInputEnumType()
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
                        Column = "ClientType",
                        ConditionType = WhereCondition.Equal,
                        Value = "Servico"
                    }
                }
            }
        };

        // Act
        var filteredResult = Mock.Clients.ApplyFilterRequest(request);
        var normalResult = Mock.Clients.Where(p => p.ClientType.Equals(ClientEnum.Servico));

        // Assert
        Helper.EnumarableAreEqual(filteredResult, normalResult);
        Assert.NotEmpty(filteredResult);
    }

    [Fact]
    public void WhereTests_ApplyFilterRequest_WhenConditionIsUnknown_ShouldThrowWithConditionAndColumn()
    {
        // Arrange
        var request = new FilterRequest
        {
            Where = new Filter {Column = "Age", Condition = "maiorque", Value = 14}
        };

        // Act
        var exception = Assert.Throws<ArgumentException>(() => Mock.Clients.ApplyFilterRequest(request));

        // Assert
        Assert.Contains("'maiorque'", exception.Message);
        Assert.Contains("'Age'", exception.Message);
    }

    [Fact]
    public void WhereTests_ApplyFilterRequest_WhenConditionTypeIsSetAfterUnknownCondition_ShouldUseConditionType()
    {
        // Arrange
        var request = new FilterRequest
        {
            Where = new Filter {Column = "Age", Condition = "maiorque", ConditionType = WhereCondition.GreaterThan, Value = 14}
        };

        // Act
        var filteredResult = Mock.Clients.ApplyFilterRequest(request);
        var normalResult = Mock.Clients.Where(x => x.Age > 14);

        // Assert
        Helper.EnumarableAreEqual(filteredResult, normalResult);
    }

    [Fact]
    public void WhereTests_ApplyFilterRequest_WhenOperatorIsUnknown_ShouldThrowWithOperator()
    {
        // Arrange
        var request = new FilterRequest
        {
            Where = new Filter
            {
                Operator = "xor",
                Filters = new List<Filter>
                {
                    new("Age", WhereCondition.GreaterThanOrEqual, 14),
                    new("Age", WhereCondition.LessThanOrEqual, 25)
                }
            }
        };

        // Act
        var exception = Assert.Throws<ArgumentException>(() => Mock.Clients.ApplyFilterRequest(request));

        // Assert
        Assert.Contains("'xor'", exception.Message);
    }

    [Fact]
    public void WhereTests_ApplyFilterRequest_WhenWhereIsNull()
    {
        // Arrange
        var request = new FilterRequest()
        {
            Where = null,
            OrderBy = new List<Order>()
            {
                new Order("Age", OrderDirection.Asc)
            },
            Limit = 10,
            PageNumber = -1
        };

        // Act
        var filteredResult = Mock.Clients.ApplyFilterRequest(request);
        var normalResult = Mock.Clients.OrderBy(x => x.Age).Take(10);

        // Assert
        Helper.EnumarableAreEqual(filteredResult, normalResult);
        Assert.NotEmpty(filteredResult);
    }
}