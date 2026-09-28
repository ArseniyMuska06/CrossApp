using Core.Domain;
using Core.Dto;

namespace Core.Import;

public static class ProductDomainMapper
{
    public static ImportResult<Product> Map(
        ImportResult<ProductDto> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var products = new List<Product>();

        // Зберігаємо помилки, які вже виявив імпортер.
        var errors = new List<string>(source.Errors);

        var acceptedLineNumbers = source.SourceLineNumbers is null
            ? null
            : new List<int>();

        for (int i = 0; i < source.Items.Count; i++)
        {
            ProductDto dto = source.Items[i];

            string location = source.SourceLineNumbers is null
                ? $"запис {i + 1}"
                : $"рядок {source.SourceLineNumbers[i]}";

            try
            {
                Product product = Product.FromDto(dto);
                products.Add(product);

                if (acceptedLineNumbers is not null)
                    acceptedLineNumbers.Add(source.SourceLineNumbers![i]);
            }
            catch (ArgumentException ex)
            {
                errors.Add(
                    $"{location}: порушення інваріанту — {ex.Message}");
            }
        }

        return new ImportResult<Product>(
            products, errors, acceptedLineNumbers);
    }
}