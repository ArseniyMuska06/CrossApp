namespace Core.Configuration;

public sealed class AppOptions
{
    public string EnvironmentName { get; init; } = "Development";
    public StorageOptions Storage { get; init; } = new();
    public LoggingOptions Logging { get; init; } = new();
    public WarehouseOptions Warehouse { get; init; } = new();
}

public sealed class StorageOptions
{
    public string DataPath { get; init; } = "data";
    public string FileName { get; init; } = "items.json";
    public string ResolveFullPath(string baseDir)
    {
        return Path.GetFullPath(Path.Combine(DataPath, FileName), baseDir);
    }
}

public sealed class LoggingOptions
{
    public string MinimumLevel { get; init; } = "Information";
}

public sealed class WarehouseOptions
{
    public string DefaultUnit { get; init; } = "шт";
}