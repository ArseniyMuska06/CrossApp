using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    public static ImportResult<object> Load(string path)
    {
        var items = new List<object>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;

                case ParseFailed failed:
                    errors.Add($"рядок {i + 1}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<object>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(
            ';',
            StringSplitOptions.TrimEntries);

        return parts switch
        {
            ["P", var id, var sku, var name, var unit, var qty]
                when id == "" || sku == "" || name == "" || unit == ""
                => new ParseFailed("обов’язкові поля товару порожні"),

            ["P", _, _, _, _, var qty]
                when !int.TryParse(
                    qty,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int q) || q < 0
                => new ParseFailed(
                    $"кількість '{qty}' не є невід’ємним цілим числом"),

            ["P", var id, var sku, var name, var unit, var qty]
                => new ParseOk(new ProductDto(
                    id,
                    sku,
                    name,
                    unit,
                    int.Parse(qty, CultureInfo.InvariantCulture))),

            ["W", var id, var name, var address]
                when id == "" || name == ""
                => new ParseFailed("ідентифікатор або назва складу порожні"),

            ["W", var id, var name, var address]
                => new ParseOk(new WarehouseDto(
                    id,
                    name,
                    address == "" ? null : address)),

            ["P", ..]
                => new ParseFailed("для товару очікується 6 колонок"),

            ["W", ..]
                => new ParseFailed("для складу очікується 4 колонки"),

            _
                => new ParseFailed("невідомий префікс типу")
        };
    }

    private abstract record ParseOutcome;

    private sealed record ParseOk(object Value) : ParseOutcome;

    private sealed record ParseFailed(string Reason) : ParseOutcome;
}