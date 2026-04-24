namespace DotNetLab.RateLimit;

/// <summary>
/// Extensões para facilitar o uso de rate limiters.
/// </summary>
public static class RateLimiterExtensions
{
    /// <summary>
    /// Executa uma ação somente se o rate limit permitir.
    /// </summary>
    public static bool ExecuteIfAllowed(this IRateLimiter limiter, string key, Action action)
    {
        var result = limiter.TryAcquireWithResult(key);
        
        if (result.IsAllowed)
        {
            action();
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Executa uma ação assíncrona somente se o rate limit permitir.
    /// </summary>
    public static async Task<bool> ExecuteIfAllowedAsync(this IRateLimiter limiter, string key, Func<Task> action)
    {
        var result = limiter.TryAcquireWithResult(key);
        
        if (result.IsAllowed)
        {
            await action();
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Executa uma função somente se o rate limit permitir, retornando o resultado.
    /// </summary>
    public static TResult? ExecuteIfAllowed<TResult>(this IRateLimiter limiter, string key, Func<TResult> func)
    {
        var result = limiter.TryAcquireWithResult(key);
        
        if (result.IsAllowed)
        {
            return func();
        }
        
        return default;
    }

    /// <summary>
    /// Executa uma função assíncrona somente se o rate limit permitir.
    /// </summary>
    public static async Task<TResult?> ExecuteIfAllowedAsync<TResult>(this IRateLimiter limiter, string key, Func<Task<TResult>> func)
    {
        var result = limiter.TryAcquireWithResult(key);
        
        if (result.IsAllowed)
        {
            return await func();
        }
        
        return default;
    }

    /// <summary>
    /// Verifica se o rate limit permite a requisição.
    /// </summary>
    public static bool IsAllowed(this IRateLimiter limiter, string key) =>
        limiter.TryAcquire(key);

    /// <summary>
    /// Retorna o número de requisições restantes.
    /// </summary>
    public static int GetRemaining(this IRateLimiter limiter, string key) =>
        limiter.GetStatus(key).Limit - limiter.GetStatus(key).CurrentCount;

    /// <summary>
    /// Retorna informações para headers HTTP de rate limit.
    /// </summary>
    public static Dictionary<string, string> GetRateLimitHeaders(this IRateLimiter limiter, string key)
    {
        var status = limiter.GetStatus(key);
        var result = limiter.TryAcquireWithResult(key);
        
        var headers = new Dictionary<string, string>
        {
            ["X-RateLimit-Limit"] = status.Limit.ToString(),
            ["X-RateLimit-Remaining"] = result.Remaining.ToString(),
            ["X-RateLimit-Reset"] = status.WindowReset.ToUnixTimeSeconds().ToString()
        };

        if (result.RetryAfter.HasValue)
        {
            headers["Retry-After"] = ((int)result.RetryAfter.Value.TotalSeconds).ToString();
        }

        return headers;
    }
}

/// <summary>
/// Builder fluente para configurar rate limiters.
/// </summary>
public class RateLimiterBuilder
{
    private RateLimiterType _type = RateLimiterType.FixedWindow;
    private int _limit = 100;
    private TimeSpan _window = TimeSpan.FromMinutes(1);
    private int _refillAmount = 1;

    public RateLimiterBuilder WithType(RateLimiterType type)
    {
        _type = type;
        return this;
    }

    public RateLimiterBuilder WithLimit(int limit)
    {
        _limit = limit;
        return this;
    }

    public RateLimiterBuilder WithWindow(TimeSpan window)
    {
        _window = window;
        return this;
    }

    public RateLimiterBuilder WithRefill(int amount)
    {
        _refillAmount = amount;
        return this;
    }

    /// <summary>
    /// Configura para 100 requisições por minuto.
    /// </summary>
    public RateLimiterBuilder PerMinute(int requests)
    {
        _limit = requests;
        _window = TimeSpan.FromMinutes(1);
        return this;
    }

    /// <summary>
    /// Configura para 1000 requisições por hora.
    /// </summary>
    public RateLimiterBuilder PerHour(int requests)
    {
        _limit = requests;
        _window = TimeSpan.FromHours(1);
        return this;
    }

    /// <summary>
    /// Configura para N requisições por segundo.
    /// </summary>
    public RateLimiterBuilder PerSecond(int requests)
    {
        _limit = requests;
        _window = TimeSpan.FromSeconds(1);
        return this;
    }

    public IRateLimiter Build()
    {
        return RateLimiterFactory.Create(new RateLimiterConfig
        {
            Type = _type,
            Limit = _limit,
            Window = _window,
            RefillAmount = _refillAmount
        });
    }
}

/// <summary>
/// Métodos de extensão para o builder.
/// </summary>
public static class RateLimiterBuilderExtensions
{
    /// <summary>
    /// Inicia a configuração com janela fixa.
    /// </summary>
    public static RateLimiterBuilder FixedWindow(this RateLimiterBuilder builder) =>
        builder.WithType(RateLimiterType.FixedWindow);

    /// <summary>
    /// Inicia a configuração com janela deslizante.
    /// </summary>
    public static RateLimiterBuilder SlidingWindow(this RateLimiterBuilder builder) =>
        builder.WithType(RateLimiterType.SlidingWindow);

    /// <summary>
    /// Inicia a configuração com token bucket.
    /// </summary>
    public static RateLimiterBuilder TokenBucket(this RateLimiterBuilder builder) =>
        builder.WithType(RateLimiterType.TokenBucket);

    /// <summary>
    /// Inicia a configuração com leaky bucket.
    /// </summary>
    public static RateLimiterBuilder LeakyBucket(this RateLimiterBuilder builder) =>
        builder.WithType(RateLimiterType.LeakyBucket);
}
