using Core.Reports;
using Xunit;

namespace Core.Tests;

public sealed class ReportServiceTests
{
    [Theory]
    [InlineData(10, 2)]
    [InlineData(1, 0)]
    public void LowStock_CountsOnlyBelowThreshold(
        int threshold,
        int expected)
    {
        var items = TestData.Items();

        var result = ReportService.LowStock(items, threshold);

        Assert.Equal(expected, result.Count);
    }

    [Fact]
    public void TopByQuantity_IsDeterministic()
    {
        var items = TestData.Items();
        var expected = new[] { "SKU-BIG", "SKU-MID" };

        var first = ReportService.TopByQuantity(items, 2)
            .Select(line => line.Sku)
            .ToArray();

        var second = ReportService.TopByQuantity(items, 2)
            .Select(line => line.Sku)
            .ToArray();

        Assert.Equal(expected, first);
        Assert.Equal(first, second);
    }
}