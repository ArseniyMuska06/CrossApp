using Core.Domain;

namespace Core.Abstractions;

public interface IStreamingCatalogImporter
{
    IAsyncEnumerable<Product> LoadStreamAsync(
        string path,
        CancellationToken ct = default);
}