using Vip.DynamicFilter.Tests.Core;
using Xunit;

namespace Vip.DynamicFilter.Tests;

public class OrderTests
{
    [Fact]
    public void OrderTests_OrderBy_WhenSingleOrderInputIsPassed_ShouldReturnOrderedQueryable()
    {
        // Arrange
        var order = new Order("Age", OrderDirection.Desc);

        // Act
        var filteredResult = Mock.Clients.OrderBy(order);
        var normalResult = Mock.Clients.OrderByDescending(x => x.Age);

        // Assert
        Helper.EnumarableAreEqual(filteredResult, normalResult);
    }

    [Fact]
    public void OrderTests_OrderBy_WhenListOfOrderInputsArePassed_ShouldReturnOrderedQueryable()
    {
        // Arrange
        var order = new List<Order>
        {
            new("Age", OrderDirection.Desc),
            new("Name", OrderDirection.Desc),
        };

        // act
        var filteredResult = Mock.Clients.OrderBy(order);
        var normalResult = Mock.Clients.OrderByDescending(p => p.Age).ThenByDescending(p => p.Name);

        // assert
        Helper.EnumarableAreEqual(filteredResult, normalResult);
        Assert.NotEmpty(filteredResult);
    }

    [Fact]
    public void OrderTests_ApplyFilterRequest_WhenOrderFilterInputIsPassed_ShouldReturnFilteredAndOrderedQueryable()
    {
        // Arrange
        var request = new FilterRequest
        {
            Where = new Filter
            {
                OperatorType = Operator.And,
                Filters = new List<Filter>
                {
                    new("Age", WhereCondition.GreaterThanOrEqual, 15),
                    new("Age", WhereCondition.LessThanOrEqual, 25),
                }
            },
            OrderBy = new List<Order>
            {
                new("Age", OrderDirection.Desc),
                new("Name", OrderDirection.Desc)
            }
        };

        // act
        var filteredResult = Mock.Clients.ApplyFilterRequest(request);
        var normalResult = Mock.Clients
            .Where(x => x.Age >= 15 && x.Age <= 25)
            .OrderByDescending(x => x.Age)
            .ThenByDescending(x => x.Name);

        // assert
        Helper.EnumarableAreEqual(filteredResult, normalResult);
        Assert.NotEmpty(filteredResult);
    }

    [Fact]
    public void OrderTests_ApplyFilterRequest_WhenJustOrderIsPassed_ShouldReturnOrderedQueryable()
    {
        // Arrange
        var request = new FilterRequest
        {
            OrderBy = new List<Order>
            {
                new("Age", OrderDirection.Desc),
                new("Name", OrderDirection.Desc)
            }
        };

        // Act
        var filteredResult = Mock.Clients.ApplyFilterRequest(request);
        var normalResult = Mock.Clients
            .OrderByDescending(p => p.Age)
            .ThenByDescending(p => p.Name);

        // Assert
        Helper.EnumarableAreEqual(filteredResult, normalResult);
    }
}