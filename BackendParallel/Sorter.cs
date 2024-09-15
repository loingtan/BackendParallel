using System.Collections.Concurrent;

namespace BackendParallel;

public class Sorter(
    BlockingCollection<(int, string)> inputQueue,
    ConcurrentQueue<(int, string)> primeFinderQueue,
    ConcurrentQueue<(int, string)> writerQueue)
{
    private const int BatchSize = 1000;

    private static int BinarySearch((int, string)[] arr, int x, int length)
    {
        int l = 0, r = length;
        while (l < r)
        {
            var m = l + (r - l) / 2;
            if (arr[m].Item1 >= x)
                r = m;
            else
                l = m + 1;
        }
        return l;
    }

    private static void InsertionSort((int, string)[] array, (int, string) key, int length)
    {   
        var index = BinarySearch(array, key.Item1, length);
        for (var i = length - 1; i >= index; i--)
        {
            array[i + 1] = array[i];
        }
        array[index] = key;
    }
    public Task SortNumbers(CancellationToken cancellationToken)
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
        }

        return Task.CompletedTask;
    }
}