using Vip.DynamicFilter.Tests.Core;
using Xunit;

namespace Vip.DynamicFilter.Tests;

public class PagingTests
{
    [Fact]
    public void PagingTests_ApplyFilterRequest_WhenDynamicQueryNetInputIsPassed_ShouldReturnOrderedQueryable()
    {
        // Arrange
        var request = new FilterRequest
        {
            Where = new Filter
            {
                OperatorType = Operator.And,
                Filters = new List<Filter>
                {
                    new("Age", WhereCondition.GreaterThanOrEqual, 35),
                    new("Age", WhereCondition.LessThanOrEqual, 56)
                }
            },
            OrderBy = new List<Order>
            {
                new("Name", OrderDirection.Desc)
            },
            PageNumber = 2,
            Limit = 2
        };

        // Act
        var filteredResult = Mock.Clients.ApplyFilterRequest(request);
        var normalResult = Mock.Clients
            .Where(x => x.Age >= 35 && x.Age <= 56)
            .OrderByDescending(x => x.Name)
            .Skip(1 * 2)
            .Take(2);

        // Assert
        Helper.EnumarableAreEqual(filteredResult, normalResult);
    }

    [Fact]
    public void PagingTests_ApplyFilterRequest_WhenDynamicQueryNetInputIsPassed_PagingIsNull_ShouldReturnOrderedQueryable()
    {
        var request = new FilterRequest
        {
            Where = new Filter
            {
                OperatorType = Operator.And,
                Filters = new List<Filter>
                {
                    new("Address.Street", WhereCondition.ContainsIgnoreCase, "rua"),
                    new("Age", WhereCondition.GreaterThanOrEqual, 36)
                }
            },
            OrderBy = new List<Order>
            {
                new("Name", OrderDirection.Desc)
            }
        };

        // Act
        var filteredResult = Mock.Clients.ApplyFilterRequest(request);
        var normalResult = Mock.Clients
            .Where(x => x.Address.Any(a => a.Street.Contains("rua", StringComparison.InvariantCultureIgnoreCase) && x.Age >= 36))
            .OrderByDescending(x => x.Name);

        // Assert
        Helper.EnumarableAreEqual(filteredResult, normalResult);
    }

    [Fact]
    public void PagingTests_ApplyFilterRequest_WhenDynamicQueryNetInputIsPassed_OrderIsNull_ShouldNotPagingTheResult()
    {
        // Arrange
        var request = new FilterRequest
        {
            Where = new Filter("Age", WhereCondition.GreaterThanOrEqual, 56),
            PageNumber = 1,
            Limit = 2
        };

        // Act
        var filteredResult = Mock.Clients.ApplyFilterRequest(request);
        var normalResult = Mock.Clients.Where(x => x.Age >= 56).Skip(0 * 2).Take(2);

        // Assert
        Helper.EnumarableAreEqual(filteredResult, normalResult);
    }
}