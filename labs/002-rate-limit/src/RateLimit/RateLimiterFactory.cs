namespace DotNetLab.RateLimit;

/// <summary>
/// Tipos de algoritmos de rate limiting disponíveis.
/// </summary>
public enum RateLimiterType
{
    /// <summary>Janela fixa - simples, pode ter spikes no limite</summary>
    FixedWindow,
    
    /// <summary>Janela deslizante - mais preciso, maior uso de memória</summary>
    SlidingWindow,
    
    /// <summary>Token Bucket - permite bursts, ideal para APIs</summary>
    TokenBucket,
    
    /// <summary>Leaky Bucket - suaviza tráfego, throughput constante</summary>
    LeakyBucket
}

/// <summary>
/// Configuração para criação de rate limiters.
/// </summary>
public record RateLimiterConfig
{
    public required RateLimiterType Type { get; init; }
    public required int Limit { get; init; }
    public required TimeSpan Window { get; init; }
    
    /// <summary>
    /// Para Token Bucket: quantidade de tokens a adicionar por janela.
    /// Padrão: 1
    /// </summary>
    public int RefillAmount { get; init; } = 1;
}

/// <summary>
/// Fábrica para criar instâncias de rate limiters.
/// </summary>
public static class RateLimiterFactory
{
    /// <summary>
    /// Cria um rate limiter baseado na configuração.
    /// </summary>
    public static IRateLimiter Create(RateLimiterConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return config.Type switch
        {
            RateLimiterType.FixedWindow => new FixedWindowRateLimiter(config.Limit, config.Window),
            RateLimiterType.SlidingWindow => new SlidingWindowRateLimiter(config.Limit, config.Window),
            RateLimiterType.TokenBucket => new TokenBucketRateLimiter(config.Limit, config.Window, config.RefillAmount),
            RateLimiterType.LeakyBucket => new LeakyBucketRateLimiter(config.Limit, config.Window),
            _ => throw new ArgumentException($"Tipo de rate limiter não suportado: {config.Type}", nameof(config))
        };
    }

    /// <summary>
    /// Cria um rate limiter com opções simplificadas.
    /// </summary>
    public static IRateLimiter Create(RateLimiterType type, int limit, TimeSpan window)
    {
        return Create(new RateLimiterConfig
        {
            Type = type,
            Limit = limit,
            Window = window
        });
    }

    /// <summary>
    /// Cria um Fixed Window rate limiter.
    /// </summary>
    public static IRateLimiter CreateFixedWindow(int limit, TimeSpan window) =>
        new FixedWindowRateLimiter(limit, window);

    /// <summary>
    /// Cria um Sliding Window rate limiter.
    /// </summary>
    public static IRateLimiter CreateSlidingWindow(int limit, TimeSpan window) =>
        new SlidingWindowRateLimiter(limit, window);

    /// <summary>
    /// Cria um Token Bucket rate limiter.
    /// </summary>
    public static IRateLimiter CreateTokenBucket(int capacity, TimeSpan refillPeriod, int refillAmount = 1) =>
        new TokenBucketRateLimiter(capacity, refillPeriod, refillAmount);

    /// <summary>
    /// Cria um Leaky Bucket rate limiter.
    /// </summary>
    public static IRateLimiter CreateLeakyBucket(int capacity, TimeSpan leakRate) =>
        new LeakyBucketRateLimiter(capacity, leakRate);
}
