using System.Collections.Concurrent;

namespace BackendParallel;

public class Sorter(
    BlockingCollection<(int, string)> inputQueue,
    ConcurrentQueue<(int, string)> primeFinderQueue,
    ConcurrentQueue<(int, string)> writerQueue)
{
    private const int BatchSize = 1000;
    private const int DelayMilliseconds = 1;
    public async Task SortNumbers(CancellationToken cancellationToken)
    {
        var batch = new List<(int, string)>(BatchSize);
        while (!cancellationToken.IsCancellationRequested)
        {
            while (batch.Count < BatchSize && inputQueue.TryTake(out var val))
            {
                batch.Add(val);
            }

            if (batch.Count == BatchSize)
            {
                batch.Sort((x, y) => x.Item1.CompareTo(y.Item1));
                foreach (var number in batch)
                {
                    primeFinderQueue.Enqueue(number);
                    writerQueue.Enqueue(number);
                }
                batch.Clear();
            }

            await Task.Delay(DelayMilliseconds, cancellationToken);
        }
    }
}