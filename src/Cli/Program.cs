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
Console.WriteLine("Імпорт товарів");
Console.WriteLine(new string('-', 52));

string path = args.Length > 0
    ? args[0]
    : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

ImportResult<ProductDto> result = ProductCsvImporter.Load(path);

Console.WriteLine($"Завантажено записів: {result.Items.Count}");

foreach (ProductDto p in result.Items.Take(5))
{
    Console.WriteLine(
        $" {p.Id,-6} {p.Sku,-10} {p.Name,-32} {p.Quantity,5} {p.Unit}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");

    foreach (string error in result.Errors)
    {
        Console.WriteLine($" ! {error}");
    }
}

return 0;