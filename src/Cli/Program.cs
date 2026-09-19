using System.Globalization;
using System.Text.Json;
using Core;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();

Console.WriteLine("CrossApp – інформація про середовище");
Console.WriteLine(new string('-', 52));

Console.WriteLine($"ОС            : {report.OsDescription}");
Console.WriteLine($"Runtime       : {report.FrameworkDescription}");
Console.WriteLine($"Архітектура   : {report.ProcessArchitecture}");
Console.WriteLine($"RID (визначено): {report.DetectedRid}");
Console.WriteLine($"RID (від .NET): {report.ReportedRid}");
Console.WriteLine($"Каталог       : {report.BaseDirectory}");
Console.WriteLine($"Збірка        : {report.BuildNote}");

Console.WriteLine();
Console.WriteLine("Імпорт даних");
Console.WriteLine(new string('-', 52));

string path = args.Length > 0
    ? args[0]
    : Path.Combine("data", "sample.csv");

bool mixed = args.Length > 1 && args[1] == "--mixed";

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

try
{
    string extension = Path.GetExtension(path).ToLowerInvariant();

    if (mixed)
    {
        if (extension != ".csv")
        {
            Console.WriteLine("Режим --mixed підтримує лише CSV.");
            return 1;
        }

        PrintResult(MixedCsvImporter.Load(path));
    }
    else
    {
        ImportResult<ProductDto> result = extension switch
        {
            ".csv" => ProductCsvImporter.Load(path),
            ".json" => ProductJsonImporter.Load(path),

            _ => throw new NotSupportedException(
                $"Непідтримуване розширення: {extension}")
        };

        PrintResult(result);
    }

    return 0;
}
catch (JsonException ex)
{
    Console.WriteLine($"Помилка формату JSON: {ex.Message}");
    return 1;
}
catch (IOException ex)
{
    Console.WriteLine($"Помилка читання файлу: {ex.Message}");
    return 1;
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("Немає доступу до файлу.");
    return 1;
}
catch (NotSupportedException ex)
{
    Console.WriteLine(ex.Message);
    return 1;
}

static void PrintResult<T>(ImportResult<T> result)
{
    Console.WriteLine($"Завантажено записів: {result.Items.Count}");

    foreach (T item in result.Items.Take(5))
    {
        string text = item switch
        {
            ProductDto p =>
                $"Товар: {p.Id,-6} {p.Sku,-10} " +
                $"{p.Name,-32} {p.Quantity,5} {p.Unit}",

            WarehouseDto w =>
                $"Склад: {w.Id,-6} {w.Name}; " +
                $"адреса: {w.Address ?? "не вказана"}",

            _ => "Невідомий тип запису"
        };

        Console.WriteLine($" {text}");
    }

    if (result.Errors.Count > 0)
    {
        Console.WriteLine($"Пропущено записів: {result.Errors.Count}");

        foreach (string error in result.Errors)
            Console.WriteLine($" ! {error}");
    }

    int accepted = result.Items.Count;
    int skipped = result.Errors.Count;
    int total = accepted + skipped;

    double errorPercent = total == 0
        ? 0
        : skipped * 100.0 / total;

    string percentText = errorPercent.ToString(
        "F2",
        CultureInfo.InvariantCulture);

    Console.WriteLine(
        $"Статистика: усього — {total}; " +
        $"прийнято — {accepted}; " +
        $"пропущено — {skipped}; " +
        $"помилок — {percentText}%");
}