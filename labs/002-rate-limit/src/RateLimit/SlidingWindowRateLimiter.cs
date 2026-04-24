using System.Collections.Concurrent;

namespace DotNetLab.RateLimit;

/// <summary>
/// Rate limiter com janela deslizante.
/// Verifica as requisições nos últimos X segundos/minutos.
/// Mais preciso que janela fixa mas consome mais memória.
/// </summary>
public class SlidingWindowRateLimiter : IRateLimiter
{
    private readonly int _maxRequests;
    private readonly TimeSpan _windowSize;
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> _buckets = new();

    public SlidingWindowRateLimiter(int maxRequests, TimeSpan windowSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRequests);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(windowSize.Ticks);
        
        _maxRequests = maxRequests;
        _windowSize = windowSize;
    }

    public bool TryAcquire(string key)
    {
        return TryAcquireWithResult(key).IsAllowed;
    }

    public RateLimitResult TryAcquireWithResult(string key)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.Subtract(_windowSize);

        var queue = _buckets.GetOrAdd(key, _ => new ConcurrentQueue<DateTime>());

        lock (queue)
        {
            // Remove timestamps fora da janela deslizante
            while (queue.TryPeek(out var timestamp) && timestamp < windowStart)
            {
                queue.TryDequeue(out _);
            }

            var count = queue.Count;
            var isAllowed = count < _maxRequests;

            if (isAllowed)
            {
                queue.Enqueue(now);
                count++;
            }

            var windowReset = isAllowed && queue.TryPeek(out var first)
                ? first.Add(_windowSize)
                : now.Add(_windowSize);

            return new RateLimitResult
            {
                IsAllowed = isAllowed,
                Remaining = Math.Max(0, _maxRequests - count),
                Limit = _maxRequests,
                RetryAfter = isAllowed ? null : windowReset - now,
                WindowReset = new DateTimeOffset(windowReset, TimeSpan.Zero)
            };
        }
    }

    public RateLimitStatus GetStatus(string key)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.Subtract(_windowSize);

        if (_buckets.TryGetValue(key, out var queue))
        {
            lock (queue)
            {
                // Remove timestamps fora da janela
                while (queue.TryPeek(out var timestamp) && timestamp < windowStart)
                {
                    queue.TryDequeue(out _);
                }

                var windowReset = queue.TryPeek(out var first)
                    ? first.Add(_windowSize)
                    : now.Add(_windowSize);

                return new RateLimitStatus
                {
                    CurrentCount = queue.Count,
                    Limit = _maxRequests,
                    WindowSize = _windowSize,
                    WindowReset = new DateTimeOffset(windowReset, TimeSpan.Zero)
                };
            }
        }

        return new RateLimitStatus
        {
            CurrentCount = 0,
            Limit = _maxRequests,
            WindowSize = _windowSize,
            WindowReset = new DateTimeOffset(now.Add(_windowSize), TimeSpan.Zero)
        };
    }
}
