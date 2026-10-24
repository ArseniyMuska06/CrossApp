using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public sealed class LinkedCancellationTests
{
    [Fact]
    public async Task ImportAsync_WhenTimeoutExpires_CancelsThroughLinkedToken()
    {
        // Arrange
        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource();

        using var linkedCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                userCts.Token,
                timeoutCts.Token);

        var store = new InMemoryCatalogStore();
        var service = new CatalogService(store);
        var importer = new WaitingImporter();

        var importTask = service.ImportAsync(
            importer,
            "unused.json",
            linkedCts.Token);

        // Act
        timeoutCts.CancelAfter(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => importTask.WaitAsync(TimeSpan.FromSeconds(10)));

        // Assert
        Assert.True(timeoutCts.IsCancellationRequested);
        Assert.False(userCts.IsCancellationRequested);
        Assert.True(linkedCts.IsCancellationRequested);
        Assert.Empty(store.List());
    }

    private sealed class WaitingImporter : ICatalogImporter
    {
        public async Task<IReadOnlyList<Product>> LoadAsync(
            string path,
            CancellationToken ct = default)
        {
            await Task.Delay(Timeout.Infinite, ct);

            return Array.Empty<Product>();
        }
    }
}