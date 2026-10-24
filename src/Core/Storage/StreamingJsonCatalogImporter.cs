using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Core.Storage;

public sealed class StreamingJsonCatalogImporter
    : IStreamingCatalogImporter
{
    public async IAsyncEnumerable<Product> LoadStreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var recordNumber = 0;

        await foreach (var dto in
            JsonSerializer.DeserializeAsyncEnumerable<ProductDto>(
                stream,
                options,
                cancellationToken: ct))
        {
            ct.ThrowIfCancellationRequested();
            recordNumber++;

            Product product;

            try
            {
                product = Product.FromDto(dto!);
            }
            catch (ArgumentException ex)
            {
                throw new DomainException(
                    $"Запис {recordNumber}: {ex.Message}", ex);
            }

            yield return product;
        }
    }
}