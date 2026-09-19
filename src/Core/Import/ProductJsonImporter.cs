using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        string json = File.ReadAllText(path);

        using JsonDocument document = JsonDocument.Parse(json);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new JsonException("Очікується масив товарів.");

        int number = 0;

        foreach (JsonElement element in document.RootElement.EnumerateArray())
        {
            number++;

            try
            {
                ProductDto? product =
                    element.Deserialize<ProductDto>(options);

                string? error = product switch
                {
                    null => "запис не містить товару",

                    { Sku: null or "" } or { Name: null or "" }
                        => "SKU або назва порожні",

                    { Quantity: < 0 }
                        => "кількість не може бути від’ємною",

                    _ => null
                };

                if (error is not null)
                {
                    errors.Add($"запис {number}: {error}");
                    continue;
                }

                items.Add(product!);
            }
            catch (JsonException)
            {
                errors.Add(
                    $"запис {number}: дані не відповідають типу ProductDto");
            }
        }

        return new ImportResult<ProductDto>(items, errors);
    }
}