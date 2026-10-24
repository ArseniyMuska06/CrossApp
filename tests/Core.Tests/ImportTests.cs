using Core;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public sealed class ImportTests
{
    [Fact]
    public async Task ImportAsync_WhenAlreadyCancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ImportAsync(
                new JsonCatalogImporter(),
                "any.json",
                cts.Token));

        Assert.Empty(store.List());
    }

    [Fact]
    public async Task LoadAsync_WithInvalidRecord_ThrowsDomainException()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            $"crossapp-test-{Guid.NewGuid():N}.json");

        const string json = """
            [
              {
                "id": "TEST-INVALID",
                "sku": "SKU-INVALID",
                "name": "",
                "unit": "шт",
                "quantity": 10
              }
            ]
            """;

        try
        {
            await File.WriteAllTextAsync(path, json);

            var importer = new JsonCatalogImporter();

            var exception = await Assert.ThrowsAsync<DomainException>(
                () => importer.LoadAsync(path));

            Assert.Contains("Запис 1:", exception.Message);
            Assert.IsAssignableFrom<ArgumentException>(
                exception.InnerException);
        }
        finally
        {
            File.Delete(path);
        }
    }
}