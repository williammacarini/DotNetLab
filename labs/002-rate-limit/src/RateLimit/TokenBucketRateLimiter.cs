using System.Collections.Concurrent;

namespace DotNetLab.RateLimit;

/// <summary>
/// Rate limiter com algoritmo Token Bucket.
/// Tokens são adicionados a uma taxa constante. Cada requisição consome um token.
/// Permite picos de tráfego até o bucket ficar vazio.
/// </summary>
public class TokenBucketRateLimiter : IRateLimiter
{
    private readonly int _capacity;
    private readonly double _refillRate;
    private readonly ConcurrentDictionary<string, BucketState> _buckets = new();

    public TokenBucketRateLimiter(int capacity, TimeSpan refillPeriod, int refillAmount = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(refillPeriod.Ticks);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(refillAmount);

        _capacity = capacity;
        _refillRate = refillAmount / refillPeriod.TotalSeconds;
    }

    public TokenBucketRateLimiter(int capacity, double refillRatePerSecond)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(refillRatePerSecond);

        _capacity = capacity;
        _refillRate = refillRatePerSecond;
    }

    public bool TryAcquire(string key)
    {
        return TryAcquireWithResult(key).IsAllowed;
    }

    public RateLimitResult TryAcquireWithResult(string key)
    {
        var now = DateTimeOffset.UtcNow;

        var bucket = _buckets.AddOrUpdate(key,
            addValueFactory: _ => new BucketState { Tokens = _capacity - 1, LastRefill = now, LastRequestAllowed = true },
            updateValueFactory: (_, existing) =>
            {
                var elapsedSeconds = (now - existing.LastRefill).TotalSeconds;
                var tokensToAdd = (int)(elapsedSeconds * _refillRate);
                var newTokens = Math.Min(_capacity, existing.Tokens + tokensToAdd);

                if (newTokens > 0)
                    return new BucketState { Tokens = newTokens - 1, LastRefill = tokensToAdd > 0 ? now : existing.LastRefill, LastRequestAllowed = true };

                return new BucketState { Tokens = 0, LastRefill = existing.LastRefill, LastRequestAllowed = false };
            });

        var remainingTokens = bucket.Tokens;
        var isAllowed = bucket.LastRequestAllowed;

        // Calcula quando terá token disponível
        TimeSpan? retryAfter = null;
        if (!isAllowed)
        {
            var tokensNeeded = 1 - remainingTokens;
            var secondsToWait = tokensNeeded / _refillRate;
            retryAfter = TimeSpan.FromSeconds(secondsToWait);
        }

        return new RateLimitResult
        {
            IsAllowed = isAllowed,
            Remaining = isAllowed ? remainingTokens : 0,
            Limit = _capacity,
            RetryAfter = retryAfter,
            WindowReset = now.Add(TimeSpan.FromSeconds(1 / _refillRate))
        };
    }

    public RateLimitStatus GetStatus(string key)
    {
        var now = DateTimeOffset.UtcNow;

        if (_buckets.TryGetValue(key, out var bucket))
        {
            var elapsedSeconds = (now - bucket.LastRefill).TotalSeconds;
            var tokensToAdd = (int)(elapsedSeconds * _refillRate);
            var currentTokens = Math.Min(_capacity, bucket.Tokens + tokensToAdd);

            return new RateLimitStatus
            {
                CurrentCount = _capacity - currentTokens,
                Limit = _capacity,
                WindowSize = TimeSpan.FromSeconds(1 / _refillRate),
                WindowReset = now.Add(TimeSpan.FromSeconds((1 - currentTokens % 1) / _refillRate))
            };
        }

        return new RateLimitStatus
        {
            CurrentCount = 0,
            Limit = _capacity,
            WindowSize = TimeSpan.FromSeconds(1 / _refillRate),
            WindowReset = now
        };
    }

    private class BucketState
    {
        public int Tokens { get; set; }
        public DateTimeOffset LastRefill { get; set; }
        public bool LastRequestAllowed { get; set; }
    }
}
