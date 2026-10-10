using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class FileSink : INotificationSink
{
    private readonly string _path;

    public FileSink(string path)
    {
        _path = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
    }

    public void Notify(CatalogChangedEventArgs change)
    {
        var line = $"{change.At:HH:mm:ss} {change.Kind,-8} " +
                   $"{change.Id} {change.Delta,4:+0;-0} " +
                   $"→ залишок {change.QuantityAfter}";

        File.AppendAllText(_path, line + Environment.NewLine);
    }
}