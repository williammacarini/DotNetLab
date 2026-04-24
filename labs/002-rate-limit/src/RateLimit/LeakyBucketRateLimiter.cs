using System.Collections.Concurrent;

namespace DotNetLab.RateLimit;

/// <summary>
/// Rate limiter com algoritmo Leaky Bucket.
/// Requisições entram em uma fila e são processadas a uma taxa constante.
/// Útil para suavizar tráfego e garantir throughput constante.
/// </summary>
public class LeakyBucketRateLimiter : IRateLimiter
{
    private readonly int _capacity;
    private readonly TimeSpan _leakRate;
    private readonly ConcurrentDictionary<string, LeakyBucketState> _buckets = new();

    public LeakyBucketRateLimiter(int capacity, TimeSpan leakRate)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(leakRate.Ticks);

        _capacity = capacity;
        _leakRate = leakRate;
    }

    public bool TryAcquire(string key)
    {
        return TryAcquireWithResult(key).IsAllowed;
    }

    public RateLimitResult TryAcquireWithResult(string key)
    {
        var now = DateTimeOffset.UtcNow;

        var bucket = _buckets.AddOrUpdate(key,
            addValueFactory: _ => new LeakyBucketState { QueueSize = 1, LastLeak = now, LastRequestAllowed = true },
            updateValueFactory: (_, existing) =>
            {
                var elapsed = now - existing.LastLeak;
                var itemsToLeak = (int)(elapsed.TotalMilliseconds / _leakRate.TotalMilliseconds);
                var newQueueSize = Math.Max(0, existing.QueueSize - itemsToLeak);
                var leakTime = itemsToLeak > 0 ? now : existing.LastLeak;

                if (newQueueSize < _capacity)
                    return new LeakyBucketState { QueueSize = newQueueSize + 1, LastLeak = leakTime, LastRequestAllowed = true };

                return new LeakyBucketState { QueueSize = newQueueSize, LastLeak = leakTime, LastRequestAllowed = false };
            });

        var isAllowed = bucket.LastRequestAllowed;

        // Calcula tempo estimado para processar
        TimeSpan? retryAfter = null;
        if (!isAllowed)
        {
            var positionInQueue = bucket.QueueSize - _capacity + 1;
            retryAfter = TimeSpan.FromMilliseconds(positionInQueue * _leakRate.TotalMilliseconds);
        }

        return new RateLimitResult
        {
            IsAllowed = isAllowed,
            Remaining = isAllowed ? _capacity - bucket.QueueSize : 0,
            Limit = _capacity,
            RetryAfter = retryAfter,
            WindowReset = now.Add(_leakRate)
        };
    }

    public RateLimitStatus GetStatus(string key)
    {
        var now = DateTimeOffset.UtcNow;

        if (_buckets.TryGetValue(key, out var bucket))
        {
            var elapsed = now - bucket.LastLeak;
            var itemsToLeak = (int)(elapsed.TotalMilliseconds / _leakRate.TotalMilliseconds);
            var currentQueueSize = Math.Max(0, bucket.QueueSize - itemsToLeak);

            var estimatedProcessingTime = TimeSpan.FromMilliseconds(currentQueueSize * _leakRate.TotalMilliseconds);

            return new RateLimitStatus
            {
                CurrentCount = currentQueueSize,
                Limit = _capacity,
                WindowSize = _leakRate,
                WindowReset = now.Add(estimatedProcessingTime)
            };
        }

        return new RateLimitStatus
        {
            CurrentCount = 0,
            Limit = _capacity,
            WindowSize = _leakRate,
            WindowReset = now
        };
    }

    private class LeakyBucketState
    {
        public int QueueSize { get; set; }
        public DateTimeOffset LastLeak { get; set; }
        public bool LastRequestAllowed { get; set; }
    }
}
