using System.Collections.Concurrent;

namespace DotNetLab.RateLimit;

/// <summary>
/// Rate limiter com janela fixa. 
/// Contador reseta a cada intervalo de tempo definido.
/// Ex: 100 requisições por minuto, reinicia no minuto seguinte.
/// </summary>
public class FixedWindowRateLimiter : IRateLimiter
{
    private readonly int _limit;
    private readonly TimeSpan _window;
    private readonly ConcurrentDictionary<string, (long WindowStart, int Count)> _requests = new();

    public FixedWindowRateLimiter(int limit, TimeSpan window)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(window.Ticks);
        
        _limit = limit;
        _window = window;
    }

    public bool TryAcquire(string key)
    {
        return TryAcquireWithResult(key).IsAllowed;
    }

    public RateLimitResult TryAcquireWithResult(string key)
    {
        var now = DateTimeOffset.UtcNow;
        var nowMs = now.ToUnixTimeMilliseconds();
        var windowStartMs = nowMs - (nowMs % (long)_window.TotalMilliseconds);
        var windowReset = DateTimeOffset.FromUnixTimeMilliseconds(windowStartMs).Add(_window);

        var result = _requests.AddOrUpdate(key,
            addValueFactory: _ => (windowStartMs, 1),
            updateValueFactory: (_, existingValue) =>
            {
                // Se está em uma nova janela, reseta o contador
                if (windowStartMs > existingValue.WindowStart)
                {
                    return (windowStartMs, 1);
                }
                // Caso contrário, incrementa
                return (existingValue.WindowStart, existingValue.Count + 1);
            });

        var count = result.Count;
        var isAllowed = count <= _limit;
        var remaining = Math.Max(0, _limit - count);
        TimeSpan? retryAfter = isAllowed ? null : windowReset - now;

        return new RateLimitResult
        {
            IsAllowed = isAllowed,
            Remaining = remaining,
            Limit = _limit,
            RetryAfter = retryAfter,
            WindowReset = windowReset
        };
    }

    public RateLimitStatus GetStatus(string key)
    {
        var now = DateTimeOffset.UtcNow;
        var nowMs = now.ToUnixTimeMilliseconds();
        var windowStartMs = nowMs - (nowMs % (long)_window.TotalMilliseconds);
        var windowReset = DateTimeOffset.FromUnixTimeMilliseconds(windowStartMs).Add(_window);

        if (_requests.TryGetValue(key, out var value) && value.WindowStart == windowStartMs)
        {
            return new RateLimitStatus
            {
                CurrentCount = value.Count,
                Limit = _limit,
                WindowSize = _window,
                WindowReset = windowReset
            };
        }

        return new RateLimitStatus
        {
            CurrentCount = 0,
            Limit = _limit,
            WindowSize = _window,
            WindowReset = windowReset
        };
    }
}
