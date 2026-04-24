namespace DotNetLab.RateLimit;

/// <summary>
/// Exemplos de uso do sistema de rate limiting.
/// </summary>
public static class RateLimiterExamples
{
    /// <summary>
    /// Exemplo 1: Rate limiting básico com Fixed Window
    /// </summary>
    public static void Example1_BasicUsage()
    {
        // Cria um rate limiter: máximo 10 requisições por minuto
        var limiter = RateLimiterFactory.CreateFixedWindow(
            limit: 10, 
            window: TimeSpan.FromMinutes(1)
        );

        var userId = "user-123";

        // Tenta fazer uma requisição
        if (limiter.TryAcquire(userId))
        {
            Console.WriteLine("Requisição permitida!");
        }
        else
        {
            Console.WriteLine("Rate limit excedido! Aguarde...");
        }

        // Ou com resultado detalhado
        var result = limiter.TryAcquireWithResult(userId);
        Console.WriteLine($"Permitido: {result.IsAllowed}");
        Console.WriteLine($"Restantes: {result.Remaining}/{result.Limit}");
        Console.WriteLine($"Reset em: {result.WindowReset}");
    }

    /// <summary>
    /// Exemplo 2: API com headers de rate limit
    /// </summary>
    public static void Example2_APIWithHeaders()
    {
        var limiter = RateLimiterFactory.CreateSlidingWindow(
            limit: 1000,
            window: TimeSpan.FromHours(1)
        );

        var apiKey = "api-key-abc";

        // Simula uma chamada de API
        var result = limiter.TryAcquireWithResult(apiKey);

        // Headers para resposta HTTP
        var headers = new Dictionary<string, string>
        {
            ["X-RateLimit-Limit"] = result.Limit.ToString(),
            ["X-RateLimit-Remaining"] = result.Remaining.ToString(),
            ["X-RateLimit-Reset"] = result.WindowReset.ToUnixTimeSeconds().ToString()
        };

        if (!result.IsAllowed)
        {
            headers["Retry-After"] = ((int)result.RetryAfter!.Value.TotalSeconds).ToString();
            // Retorna HTTP 429 Too Many Requests
        }

        Console.WriteLine("Headers de resposta:");
        foreach (var header in headers)
        {
            Console.WriteLine($"  {header.Key}: {header.Value}");
        }
    }

    /// <summary>
    /// Exemplo 3: Proteção de endpoint com diferentes limites
    /// </summary>
    public static void Example3_EndpointProtection()
    {
        // Login: limite mais restritivo (5 tentativas/minuto)
        var loginLimiter = RateLimiterFactory.CreateTokenBucket(
            capacity: 5,
            refillPeriod: TimeSpan.FromMinutes(1)
        );

        // API geral: limite mais permissivo
        var apiLimiter = RateLimiterFactory.CreateSlidingWindow(
            limit: 100,
            window: TimeSpan.FromMinutes(1)
        );

        string ip = "192.168.1.1";

        // Proteção contra brute force no login
        if (!loginLimiter.TryAcquire($"login:{ip}"))
        {
            Console.WriteLine("Muitas tentativas de login. Tente novamente mais tarde.");
            return;
        }

        // Após login, verifica limite da API
        if (!apiLimiter.TryAcquire($"api:{ip}"))
        {
            Console.WriteLine("Limite de requisições excedido.");
            return;
        }

        Console.WriteLine("Requisição processada com sucesso!");
    }

    /// <summary>
    /// Exemplo 4: Builder fluente
    /// </summary>
    public static void Example4_FluentBuilder()
    {
        // API pública: 100 req/min com janela deslizante
        var publicApiLimiter = new RateLimiterBuilder()
            .SlidingWindow()
            .PerMinute(100)
            .Build();

        // Webhook: 10 req/seg com token bucket (permite bursts)
        var webhookLimiter = new RateLimiterBuilder()
            .TokenBucket()
            .PerSecond(10)
            .WithRefill(10)
            .Build();

        // Processamento de fila: throughput constante
        var queueLimiter = new RateLimiterBuilder()
            .LeakyBucket()
            .WithLimit(100)
            .WithWindow(TimeSpan.FromSeconds(1))
            .Build();

        Console.WriteLine("Rate limiters configurados com sucesso!");
    }

    /// <summary>
    /// Exemplo 5: Uso com extensions
    /// </summary>
    public static void Example5_ExtensionMethods()
    {
        var limiter = RateLimiterFactory.CreateFixedWindow(5, TimeSpan.FromMinutes(1));
        var userId = "user-456";

        // Executa ação apenas se permitido
        var success = limiter.ExecuteIfAllowed(userId, () =>
        {
            Console.WriteLine("Processando pagamento...");
        });

        if (!success)
        {
            Console.WriteLine("Não foi possível processar: rate limit excedido");
        }

        // Com função que retorna valor
        var data = limiter.ExecuteIfAllowed(userId, () =>
        {
            return "Dados sensíveis";
        });

        if (data is not null)
        {
            Console.WriteLine($"Dados obtidos: {data}");
        }

        // Verifica status
        var remaining = limiter.GetRemaining(userId);
        Console.WriteLine($"Requisições restantes: {remaining}");
    }

    /// <summary>
    /// Exemplo 6: Múltiplos níveis de rate limit
    /// </summary>
    public static void Example6_MultiTierRateLimit()
    {
        // Nível 1: Global (por IP)
        var globalLimiter = RateLimiterFactory.CreateFixedWindow(
            limit: 10000,
            window: TimeSpan.FromHours(1)
        );

        // Nível 2: Por usuário
        var userLimiter = RateLimiterFactory.CreateSlidingWindow(
            limit: 100,
            window: TimeSpan.FromMinutes(1)
        );

        // Nível 3: Por endpoint específico
        var endpointLimiter = RateLimiterFactory.CreateTokenBucket(
            capacity: 10,
            refillPeriod: TimeSpan.FromSeconds(1),
            refillAmount: 10
        );

        string ip = "192.168.1.100";
        string userId = "user-789";
        string endpoint = "/api/expensive-operation";

        // Verifica todos os níveis
        bool allowed = globalLimiter.TryAcquire($"global:{ip}") &&
                      userLimiter.TryAcquire($"user:{userId}") &&
                      endpointLimiter.TryAcquire($"endpoint:{endpoint}:{userId}");

        if (allowed)
        {
            Console.WriteLine("Todas as verificações passaram!");
        }
        else
        {
            Console.WriteLine("Um ou mais rate limits foram excedidos.");
        }
    }

    /// <summary>
    /// Exemplo 7: Comparação dos algoritmos
    /// </summary>
    public static void Example7_AlgorithmComparison()
    {
        Console.WriteLine("=== Comparação de Algoritmos ===\n");

        Console.WriteLine("1. Fixed Window:");
        Console.WriteLine("   - Simples e eficiente");
        Console.WriteLine("   - Pode permitir 2x requisições no limite da janela");
        Console.WriteLine("   - Uso: Cache, métricas simples\n");

        Console.WriteLine("2. Sliding Window:");
        Console.WriteLine("   - Mais preciso que Fixed Window");
        Console.WriteLine("   - Consome mais memória (armazena timestamps)");
        Console.WriteLine("   - Uso: APIs REST, proteção de endpoints\n");

        Console.WriteLine("3. Token Bucket:");
        Console.WriteLine("   - Permite bursts de tráfego");
        Console.WriteLine("   - Ideal para APIs que precisam lidar com picos");
        Console.WriteLine("   - Uso: APIs públicas, webhooks\n");

        Console.WriteLine("4. Leaky Bucket:");
        Console.WriteLine("   - Suaviza tráfego para taxa constante");
        Console.WriteLine("   - Útil para proteger serviços downstream");
        Console.WriteLine("   - Uso: Filas, processamento batch, integrações");
    }

    /// <summary>
    /// Demonstração completa com todos os algoritmos.
    /// </summary>
    public static void RunAllExamples()
    {
        Console.WriteLine("=== DotNetLab.RateLimit - Exemplos ===\n");
        
        Example1_BasicUsage();
        Console.WriteLine();
        
        Example2_APIWithHeaders();
        Console.WriteLine();
        
        Example3_EndpointProtection();
        Console.WriteLine();
        
        Example4_FluentBuilder();
        Console.WriteLine();
        
        Example5_ExtensionMethods();
        Console.WriteLine();
        
        Example6_MultiTierRateLimit();
        Console.WriteLine();
        
        Example7_AlgorithmComparison();
    }
}
