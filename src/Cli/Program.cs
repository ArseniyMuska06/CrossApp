using System.Globalization;
using System.Text.Json;
using Core;
using Core.Dto;
using Core.Import;
using Core.Domain;
using Core.Abstractions;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

if (args.Length == 0 ||
    (args.Length == 1 && args[0] == "--file"))
{
    bool useFile = args.Contains("--file");

    string dataPath = Path.Combine(
        AppContext.BaseDirectory, "data", "catalog.json");

    ICatalogStore store = useFile
        ? new FileCatalogStore(dataPath)
        : new InMemoryCatalogStore(SampleData.Products());

    var service = new CatalogService(store);

    Console.WriteLine($"Сховище: {store.GetType().Name}");

    if (useFile)
        Console.WriteLine($"Файл даних: {dataPath}");

    var created = service.Add(
        "SKU-101", "Кабель UTP cat6", "м", 50);

    Console.WriteLine(
        $"Додано: {created.Name}, залишок {created.Quantity} {created.Unit}");

    service.Receive(created.Id, 25);
    Console.WriteLine($"Після приходу 25: {created.Quantity} {created.Unit}");

    service.Issue(created.Id, 10);
    Console.WriteLine($"Після видачі 10: {created.Quantity} {created.Unit}");

    Console.WriteLine();
    Console.WriteLine("Пошук за id:");

    var found = service.Find(created.Id);

    if (found is not null)
    {
        Console.WriteLine(
            $"Знайдено: {found.Id} {found.Name}, " +
            $"{found.Quantity} {found.Unit}");
    }

    Console.WriteLine();
    Console.WriteLine("Список товарів:");

    foreach (var p in service.All())
    {
        Console.WriteLine(
            $" {p.Id,-8} {p.Sku,-10} {p.Name,-25} " +
            $"{p.Quantity,5} {p.Unit}");
    }

    Console.WriteLine();
    Console.WriteLine("Сценарій відмови: неіснуючий id");

    try
    {
        service.Receive("NOT-FOUND", 10);
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Помилка: {ex.Message}");
    }

    return 0;
}

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
Console.WriteLine("Доменна модель — склад");
Console.WriteLine(new string('-', 52));

Console.WriteLine("=== Сценарій 1: успіх ===");

Product product = Product.Create(
    "P-001", "sku-001", "Цемент М400 25кг", "шт", 100);

Console.WriteLine(
    $"Початковий залишок {product.Sku}: {product.Quantity} {product.Unit}");

product.RegisterArrival(50);
Console.WriteLine(
    $"Після приходу 50: {product.Quantity} {product.Unit}");

product.Issue(30);
Console.WriteLine(
    $"Після видачі 30: {product.Quantity} {product.Unit}");

Console.WriteLine();
Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");

TryDo("Видача більша за залишок",
    () => product.Issue(1000));

TryDo("Порожній SKU",
    () => Product.Create("P-002", " ", "Пісок", "т", 10));

TryDo("Від'ємний початковий залишок",
    () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5));

TryDo("Нульова кількість приходу",
    () => product.RegisterArrival(0));

TryDo("Від'ємна кількість видачі",
    () => product.Issue(-10));

Console.WriteLine();
Console.WriteLine(
    $"Залишок після всіх відмов: {product.Quantity} {product.Unit}");

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

        ImportResult<Product> domainResult = ProductDomainMapper.Map(result);

        Console.WriteLine("Результат імпорту та перевірки доменних правил:");
        PrintResult(domainResult);
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
            Product p =>
                $"Товар: {p.Id,-6} {p.Sku,-10} " +
                $"{p.Name,-32} {p.Quantity,5} {p.Unit}",

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

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine(
            $"{title}: виняток НЕ спрацював — перевірте інваріант!");
    }
    catch (Exception ex)
    {
        Console.WriteLine(
            $"{title}: {ex.GetType().Name} — {ex.Message}");
    }
}