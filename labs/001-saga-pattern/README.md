# Lab 001 — Saga Pattern

Implementação do padrão Saga usando **MassTransit** com state machine em memória.

## Conceito

O Saga Pattern coordena transações distribuídas entre múltiplos serviços sem usar commits distribuídos (2PC). Cada etapa do processo publica um evento; a saga escuta esses eventos e avança o estado.

Neste lab, o fluxo simula o processamento de um pedido:

```
POST /orders
    → publica OrderSubmitted
    → OrderStateMachine recebe → estado: Processing
    → PaymentConsumer processa → publica PaymentProcessed
    → OrderStateMachine recebe → estado: Completed
```

## Como rodar

```bash
# da raiz do projeto
make run LAB=001-saga-pattern

# ou direto no diretório do lab
cd labs/001-saga-pattern
dotnet run --project src/SagaPattern
```

A API sobe em `http://localhost:5000`. Para disparar um pedido:

```bash
curl -X POST http://localhost:5000/orders \
  -H "Content-Type: application/json" \
  -d '{"amount": 100, "paymentMethod": "CreditCard"}'
```

## Arquivos

| Arquivo | Descrição |
|---------|-----------|
| `Contracts.cs` | Interfaces das mensagens (OrderSubmitted, PaymentProcessed, etc.) |
| `OrderState.cs` | Estado da saga (id, status, timestamps) |
| `OrderStateMachine.cs` | Máquina de estados com MassTransit |
| `PaymentConsumer.cs` | Consumidor que processa o pagamento |
| `Program.cs` | Host ASP.NET Core, configura MassTransit in-memory |

## Referências

- [MassTransit Sagas](https://masstransit.io/documentation/patterns/saga)
- [Saga Pattern — microservices.io](https://microservices.io/patterns/data/saga.html)
