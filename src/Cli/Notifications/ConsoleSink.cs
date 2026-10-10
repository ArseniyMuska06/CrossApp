using Core.Abstractions;
using Core.Events;

namespace Cli.Notifications;

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(CatalogChangedEventArgs change)
    {
        var line = $"{change.At:HH:mm:ss} {change.Kind,-8} " +
                   $"{change.Id} {change.Delta,4:+0;-0} " +
                   $"→ залишок {change.QuantityAfter}";

        Console.WriteLine(line);
    }
}