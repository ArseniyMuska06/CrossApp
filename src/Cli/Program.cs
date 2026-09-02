using System.Runtime.InteropServices;
using System.Text.Json;

var information = new
{
    Project = "CrossApp – практикум з крос-платформного програмування",
    Student = "Муска Арсеній, група ФЕІ-31",
    OSDescription = RuntimeInformation.OSDescription,
    OSVersion = Environment.OSVersion.ToString(),
    ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
    DotNetVersion = Environment.Version.ToString(),
    Runtime = RuntimeInformation.FrameworkDescription,
    ApplicationDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory,
    Domain = "Склад (товари, партії, залишки, переміщення)"
};

if (args.Contains("--json"))
{
    Console.WriteLine(JsonSerializer.Serialize(information));
}
else
{
    Console.WriteLine(information.Project);
    Console.WriteLine($"Студент: {information.Student}");
    Console.WriteLine(new string('-', 52));

    Console.WriteLine($"ОС (OSDescription)    : {information.OSDescription}");
    Console.WriteLine($"ОС (Environment)      : {information.OSVersion}");
    Console.WriteLine($"Архітектура процесу   : {information.ProcessArchitecture}");
    Console.WriteLine($"Версія .NET (CLR)     : {information.DotNetVersion}");
    Console.WriteLine($"Runtime               : {information.Runtime}");
    Console.WriteLine($"Каталог застосунку    : {information.ApplicationDirectory}");
    Console.WriteLine($"Поточний каталог      : {information.CurrentDirectory}");

    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область: {information.Domain}");
}