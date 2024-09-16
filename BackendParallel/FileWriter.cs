using System.Collections.Concurrent;
using System.Text;

namespace BackendParallel;

public class FileWriter(
    ConcurrentQueue<(int, string)> inputQueue,
    string filePath,
    string fileType,
    SystemMonitor monitor)
{
    private const int BatchSize = 100; 
    private const int DelayMilliseconds = 1; 

    public async Task WriteNumbersAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), filePath);

        await using var writer = new StreamWriter(path, append: true);
        writer.AutoFlush = true;

        var sb = new StringBuilder();
        var count = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (inputQueue.TryDequeue(out var val))
            {
                sb.AppendLine($"{val.Item1}, {val.Item2}");
                count++;

                if (count >= BatchSize)
                {
                    await writer.WriteAsync(sb.ToString());
                    sb.Clear();
                    count = 0;
                    
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
            }
            else
            {
                if (sb.Length > 0)
                {
                    await writer.WriteAsync(sb.ToString());
                    sb.Clear();
                    count = 0;
                }

                await Task.Delay(DelayMGitilliseconds, cancellationToken);
            }
        }
        
        if (sb.Length > 0)
        {
            await writer.WriteAsync(sb.ToString());
        }
    }
}