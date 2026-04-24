# Lab 002 — Rate Limiting

Implementação manual dos 4 principais algoritmos de rate limiting, sem dependências externas.

## Algoritmos

| Algoritmo | Classe | Característica |
|-----------|--------|----------------|
| Fixed Window | `FixedWindowRateLimiter` | Janela de tempo fixa. Simples, mas permite burst na virada da janela. |
| Sliding Window | `SlidingWindowRateLimiter` | Janela deslizante. Distribui melhor as requisições ao longo do tempo. |
| Token Bucket | `TokenBucketRateLimiter` | Tokens adicionados a taxa constante. Permite bursts controlados. |
| Leaky Bucket | `LeakyBucketRateLimiter` | Requisições entram em fila e saem a taxa constante. Suaviza o tráfego. |

## Como rodar os testes

```bash
# da raiz do projeto
make test LAB=002-rate-limit

# watch mode (TDD)
make watch LAB=002-rate-limit

# ou direto no diretório do lab
cd labs/002-rate-limit
dotnet test
```

## Como usar

```csharp
// Fixed Window: 100 req por minuto
var limiter = new FixedWindowRateLimiter(100, TimeSpan.FromMinutes(1));

if (limiter.TryAcquire("user-123"))
{
    // processa a requisição
}

// Com detalhes do resultado
var result = limiter.TryAcquireWithResult("user-123");
if (!result.IsAllowed)
    Console.WriteLine($"Tente novamente em {result.RetryAfter}");

// Token Bucket: capacidade 10, reabastece 1 token/seg
var tokenBucket = new TokenBucketRateLimiter(10, TimeSpan.FromSeconds(1), 1);

// Via factory
var slidingWindow = RateLimiterFactory.CreateSlidingWindow(50, TimeSpan.FromMinutes(1));
```

## Arquivos

| Arquivo | Descrição |
|---------|-----------|
| `IRateLimiter.cs` | Contrato comum + `RateLimitResult` + `RateLimitStatus` |
| `FixedWindowRateLimiter.cs` | Implementação Fixed Window |
| `SlidingWindowRateLimiter.cs` | Implementação Sliding Window |
| `TokenBucketRateLimiter.cs` | Implementação Token Bucket |
| `LeakyBucketRateLimiter.cs` | Implementação Leaky Bucket |
| `RateLimiterFactory.cs` | Factory com métodos de conveniência |
| `RateLimiterExtensions.cs` | Extension methods |
| `RateLimiterExamples.cs` | Exemplos de uso comentados |

## Referências

- [Rate Limiting Algorithms — Stripe Engineering](https://stripe.com/blog/rate-limiters)
- [An Introduction to Rate Limiting — Kong](https://konghq.com/blog/engineering/how-to-design-a-scalable-rate-limiting-algorithm)
