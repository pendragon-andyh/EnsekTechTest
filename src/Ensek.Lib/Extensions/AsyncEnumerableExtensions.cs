namespace Ensek.Lib.Extensions;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public static class AsyncEnumerableExtensions
{
    public static Task CreateProducerTask<T>(this IAsyncEnumerable<T> asyncSource, int boundedCapacity, out IEnumerable<T> consumerEnumerable, CancellationToken cancellationToken)
    {
        var blockingCollection = new BlockingCollection<T>(boundedCapacity);
        consumerEnumerable = blockingCollection.GetConsumingEnumerable(cancellationToken);

        var asyncEnumerator = asyncSource.GetAsyncEnumerator(cancellationToken);
        return CreateProducerTask(asyncEnumerator, blockingCollection, cancellationToken).GetAwaiter().GetResult();
    }

    private static async Task<Task> CreateProducerTask<T>(IAsyncEnumerator<T> sourceEnumerator, BlockingCollection<T> blockingCollection, CancellationToken cancellationToken)
    {
        // Start by reading on the current thread. Source exceptions are more likely within the first couple of rows.
        // This keeps the stack trace relatively sane.
        for (; blockingCollection.Count < blockingCollection.BoundedCapacity && await sourceEnumerator.MoveNextAsync();)
        {
            blockingCollection.Add(sourceEnumerator.Current, cancellationToken);
        }

        // If the source has already been exhausted then signal finished and return a dummy task.
        if (blockingCollection.Count < blockingCollection.BoundedCapacity)
        {
            blockingCollection.CompleteAdding();
            return Task.CompletedTask;
        }

        // Otherwise we spin-up a separate task to act as the producer for the remainder of the source.
        var producerTask = Task.Run(
            async () =>
            {
                try
                {
                    // Pump remaining source items into the blocking collection.
                    for (; await sourceEnumerator.MoveNextAsync();)
                    {
                        blockingCollection.Add(sourceEnumerator.Current, cancellationToken);
                    }
                }
                finally
                {
                    // Signal that we have finished adding.
                    blockingCollection.CompleteAdding();
                }
            },
            cancellationToken);

        // Return the IEnumerable that will consume the collection.
        return producerTask;
    }
}