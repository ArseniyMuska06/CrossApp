using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public sealed class CatalogServiceTests
{
    [Fact]
    public void Receive_WithNegativeQuantity_Throws_AndKeepsState()
    {
        // Arrange
        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);
        var product = service.Add("SKU-1", "Кабель", "м", 10);

        // Act
        Assert.Throws<ArgumentOutOfRangeException>(
            () => service.Receive(product.Id, -5));

        // Assert
        Assert.Equal(10, store.GetById(product.Id)!.Quantity);
    }

    [Fact]
    public void Add_RaisesChangedExactlyOnce()
    {
        // Arrange
        var service = new CatalogService(new InMemoryCatalogStore());
        var events = new List<CatalogChangedEventArgs>();

        service.Changed += (_, e) => events.Add(e);

        // Act
        var product = service.Add("SKU-2", "Гвинт", "шт", 7);

        // Assert
        var change = Assert.Single(events);

        Assert.Equal(CatalogChangeKind.Added, change.Kind);
        Assert.Equal(product.Id, change.Id);
        Assert.Equal(7, change.Delta);
        Assert.Equal(7, change.QuantityAfter);
    }
}