using Xunit;

namespace Vip.DynamicFilter.Tests.Core;

public static class Helper
{
    public static void EnumarableAreEqual<T>(IEnumerable<T> firstQueryable, IEnumerable<T> secondQueryable)
    {
        var firstList = firstQueryable.ToList();
        var secondList = secondQueryable.ToList();

        Assert.Equal(firstList.Count, secondList.Count);

        for (var i = 0; i < firstList.Count; i++) Assert.Equal(firstList[i], secondList[i]);
    }
}