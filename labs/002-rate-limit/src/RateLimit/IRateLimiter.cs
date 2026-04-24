namespace DotNetLab.RateLimit;

public interface IRateLimiter
{
    /// <summary>
    /// Tenta adquirir uma permissão para o identificador especificado.
    /// </summary>
    /// <param name="key">Identificador único (usuário, IP, endpoint, etc.)</param>
    /// <returns>True se a requisição é permitida, false se excedeu o limite</returns>
    bool TryAcquire(string key);

    /// <summary>
    /// Tenta adquirir uma permissão e retorna metadados sobre o estado do rate limit.
    /// </summary>
    RateLimitResult TryAcquireWithResult(string key);

    /// <summary>
    /// Retorna o estado atual do rate limit para o identificador.
    /// </summary>
    RateLimitStatus GetStatus(string key);
}

/// <summary>
/// Resultado de uma tentativa de aquisição.
/// </summary>
public record RateLimitResult
{
    public required bool IsAllowed { get; init; }
    public required int Remaining { get; init; }
    public required int Limit { get; init; }
    public required TimeSpan? RetryAfter { get; init; }
    public required DateTimeOffset WindowReset { get; init; }
}

/// <summary>
/// Status atual do rate limiter para um identificador.
/// </summary>
public record RateLimitStatus
{
    public required int CurrentCount { get; init; }
    public required int Limit { get; init; }
    public required TimeSpan WindowSize { get; init; }
    public required DateTimeOffset WindowReset { get; init; }
}
