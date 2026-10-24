using System.Globalization;
using System.Text.Json;
using Core;
using Core.Dto;
using Core.Import;
using Core.Domain;
using Core.Abstractions;
using Core.Services;
using Core.Events;
using Cli;
using Cli.Notifications;
using Core.Reports;
using Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

if (args.Contains("benchmark-reports"))
{
    ReportBenchmark.Run();
    return 0;
}

if (args.Contains("demo-deferred"))
{
    var demoStore = StoreFactory.Create(args);
    var demoService = new CatalogService(demoStore);

    DeferredExecutionDemo.Run(demoService.All());

    return 0;
}

var command = args.FirstOrDefault(a => !a.StartsWith("--"))
    ?? "report-top";

if (command is "import-async" or "import-stream")
{
    if (args.Length < 2 || args[1].StartsWith("--"))
    {
        Console.WriteLine(
            "Використання: import-async|import-stream <шлях до JSON> [--file] [--timeout]");
        return 1;
    }

    var dataPath = args[1];
    var importStore = StoreFactory.Create(args);
    var importService = new CatalogService(importStore);

    using var userCts = new CancellationTokenSource();
    using var timeoutCts = new CancellationTokenSource();

    using var cts = CancellationTokenSource.CreateLinkedTokenSource(
        userCts.Token,
        timeoutCts.Token);

    ConsoleCancelEventHandler cancelHandler = (_, e) =>
    {
        e.Cancel = true;
        userCts.Cancel();
    };

    Console.CancelKeyPress += cancelHandler;

    try
    {
        if (args.Contains("--timeout"))
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(2));

        IProgress<int> progress = new ImportProgress(percent =>
        {
            Console.Write($"\rХід імпорту: {percent,3}%");
        });

        int added;

        try
        {
            added = command == "import-stream"
                ? await importService.ImportStreamAsync(
                    new StreamingJsonCatalogImporter(),
                    dataPath,
                    cts.Token)
                : await importService.ImportAsync(
                    new JsonCatalogImporter(),
                    dataPath,
                    cts.Token,
                    progress);
        }
        finally
        {
            if (command == "import-async")
                Console.WriteLine();
        }

        Console.WriteLine($"Імпортовано записів: {added}");
        return 0;
    }
    catch (OperationCanceledException)
    {
        if (userCts.IsCancellationRequested)
            Console.WriteLine("Імпорт скасовано користувачем.");
        else if (timeoutCts.IsCancellationRequested)
            Console.WriteLine("Імпорт скасовано: завершився таймаут.");
        else
            Console.WriteLine("Імпорт скасовано.");

        return 0;
    }
    catch (DomainException ex)
    {
        Console.WriteLine($"Дані відхилено: {ex.Message}");
        return 1;
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine($"Файл не знайдено: {dataPath}");
        return 1;
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
    finally
    {
        Console.CancelKeyPress -= cancelHandler;
    }
}

if (command.StartsWith("report-", StringComparison.Ordinal))
{
    var reportStore = StoreFactory.Create(args);
    var reportService = new CatalogService(reportStore);
    var items = reportService.All();

    switch (command)
    {
        case "report-top":
            Print(ReportService.TopByQuantity(items, 5));
            break;

        case "report-low":
            Print(ReportService.LowStock(items, 10));
            break;

        case "report-groups":
            foreach (var g in ReportService.ByUnit(items))
            {
                Console.WriteLine(
                    $" {g.Key,-6} позицій: {g.Count,3} " +
                    $"сумарно: {g.TotalQuantity,6}");
            }
            break;

        case "report-sum":
            var t = ReportService.Totals(items);

            Console.WriteLine(
                $"Позицій: {t.Items}, всього: {t.Total}, " +
                $"середнє: {t.Average:F2}, макс: {t.Max}");
            break;

        case "report-movements":
            foreach (var row in ReportService.ByMovements(
                items, SampleData.Movements()))
            {
                Console.WriteLine(
                    $" {row.Sku,-10} {row.Name,-22} " +
                    $"переміщень: {row.MovementCount,3} " +
                    $"зміна: {row.TotalDelta,6:+0;-0;0}");
            }
            break;

        case "report-top-movements":
            foreach (var row in ReportService.TopByMovementCount(
                items, SampleData.Movements(), 5))
            {
                Console.WriteLine(
                    $" {row.Sku,-10} {row.Name,-22} " +
                    $"переміщень: {row.MovementCount,3} " +
                    $"зміна: {row.TotalDelta,6:+0;-0;0}");
            }
            break;

        case "report-groups-query":
            foreach (var g in ReportService.ByUnitQuery(items))
            {
                Console.WriteLine(
                    $" {g.Key,-6} позицій: {g.Count,3} " +
                    $"сумарно: {g.TotalQuantity,6}");
            }
            break;

        default:
            Console.WriteLine(
                "report-top | report-low | report-groups | " +
                "report-sum | report-movements | " +
                "report-top-movements | report-groups-query");
            break;
    }

    return 0;
}

if (command == "demo-events")
{
    ICatalogStore store = StoreFactory.Create(args);
    var service = new CatalogService(store);

    var logPath = Path.Combine(
        AppContext.BaseDirectory,
        "logs",
        "app.log");

    Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

    void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
    {
        var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} → залишок {e.QuantityAfter}";

        Console.WriteLine(line);
        File.AppendAllText(logPath, line + Environment.NewLine);
    }

    service.Changed += OnCatalogChanged;

    int changes = 0;
    service.Changed += (_, _) => changes++;

    bool useFile = args.Contains("--file");

    Console.WriteLine(
        useFile
            ? "Сховище: CachingCatalogStore → FileCatalogStore"
            : "Сховище: CachingCatalogStore → InMemoryCatalogStore");

    if (useFile)
    {
        string dataPath = Path.Combine(
            AppContext.BaseDirectory, "data", "catalog.json");

        Console.WriteLine($"Файл даних: {dataPath}");
    }

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
    Console.WriteLine("Перевірка кешування:");

    var firstList = service.All();
    var secondList = service.All();

    Console.WriteLine(
        $"Повторно використано кеш списку: " +
        $"{ReferenceEquals(firstList, secondList)}");

    // Нульовий залишок допустимий за правилами Product.
    var cacheTest = service.Add(
        "SKU-CACHE", "Тестовий товар", "шт", 0);

    var refreshedList = service.All();

    Console.WriteLine(
        $"Кеш оновлено після додавання: " +
        $"{!ReferenceEquals(secondList, refreshedList)}");

    Console.WriteLine(
        $"Кількість товарів збільшилася на 1: " +
        $"{refreshedList.Count == firstList.Count + 1}");

    store.Remove(cacheTest.Id);

    Console.WriteLine();
    Console.WriteLine("Пошук товарів із залишком менше 50:");

    var lowStock = service.Search(p => p.Quantity < 50);

    if (lowStock.Count == 0)
        Console.WriteLine("Товарів не знайдено.");

    foreach (var p in lowStock)
    {
        Console.WriteLine(
            $" {p.Sku,-10} {p.Name,-25} {p.Quantity,5} {p.Unit}");
    }

    Console.WriteLine();
    Console.WriteLine("Пошук товарів за назвою «кабель»:");

    var cables = service.Search(p =>
        p.Name.Contains("кабель", StringComparison.OrdinalIgnoreCase));

    if (cables.Count == 0)
        Console.WriteLine("Товарів не знайдено.");

    foreach (var p in cables)
    {
        Console.WriteLine(
            $" {p.Sku,-10} {p.Name,-25} {p.Quantity,5} {p.Unit}");
    }

    Console.WriteLine();

    Console.WriteLine("Пошук товарів із залишком менше 10:");

    var veryLowStock = service.FindAll(p => p.Quantity < 10);

    if (veryLowStock.Count == 0)
    {
        Console.WriteLine("Товарів не знайдено.");
    }

    foreach (var p in veryLowStock)
    {
        Console.WriteLine(
            $" {p.Sku,-10} {p.Name,-25} {p.Quantity,5} {p.Unit}");
    }

    Console.WriteLine();

    Console.WriteLine("Сценарій відмови: неіснуючий id");

    try
    {
        service.Receive("NOT-FOUND", 10);
    }
    catch (DomainException ex)
    {
        Console.WriteLine($"Помилка: {ex.Message}");
    }

    Console.WriteLine();
    Console.WriteLine("Перевірка відписки:");

    Console.WriteLine($"Подій отримано: {changes}");
    Console.WriteLine($"Залишок до відписки: {created.Quantity} {created.Unit}");

    service.Changed -= OnCatalogChanged;

    service.Receive(created.Id, 5);

        Console.WriteLine($"Залишок після операції: {created.Quantity} {created.Unit}");
        Console.WriteLine($"Подій отримано: {changes}");

        Console.WriteLine();
        Console.WriteLine("Додаткове завдання: INotificationSink");

        Console.WriteLine("ConsoleSink:");

        var consoleService = new CatalogService(store, new ConsoleSink());
        consoleService.Receive(created.Id, 1);

        Console.WriteLine(
            $"Залишок після ConsoleSink: {created.Quantity} {created.Unit}");

        Console.WriteLine();
        Console.WriteLine("FileSink:");

        var sinkLogPath = Path.Combine(
            AppContext.BaseDirectory,
            "logs",
            "sink.log");

        var fileService = new CatalogService(store, new FileSink(sinkLogPath));
        fileService.Receive(created.Id, 2);

        Console.WriteLine(
            $"Залишок після FileSink: {created.Quantity} {created.Unit}");

        Console.WriteLine($"Журнал FileSink: {sinkLogPath}");

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
            ".csv" => ProductCsvImporter.Load(
                path,
                message => Console.WriteLine($"[Імпорт] Пропущено {message}")),
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

static void Print(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
    {
        Console.WriteLine(
            $" {l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
    }
}