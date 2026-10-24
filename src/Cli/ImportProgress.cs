namespace Cli;

internal sealed class ImportProgress(Action<int> handler)
    : IProgress<int>
{
    public void Report(int value)
    {
        handler(value);
    }
}