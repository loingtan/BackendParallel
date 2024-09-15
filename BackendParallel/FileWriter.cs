using System.Collections.Concurrent;

namespace BackendParallel;

public class FileWriter(
    ConcurrentQueue<(int, string)> inputQueue,
    string filePath,
    string fileType,
    SystemMonitor monitor)
{
    public async Task WriteNumbersAsync(CancellationToken cancellationToken)
    {   
        var path = Path.Combine(Directory.GetCurrentDirectory() , filePath);
        await using var writer = new StreamWriter(path, append: true);
        writer.AutoFlush = true;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (inputQueue.TryDequeue(out var val))
            {
                await writer.WriteLineAsync($"{val.Item1}, {val.Item2}");

                switch (fileType)
                {
                    case "sorted":
                        monitor.IncrementSortedWriteCount();
                        break;
                    case "prime":
                        monitor.IncrementPrimeWriteCount();
                        break;
                }
            }
            else
            {
                await Task.Delay(1, cancellationToken);
            }
        }
    }
}