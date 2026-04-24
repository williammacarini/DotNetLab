namespace DotNetLab.Labs.SagaPattern;

using MassTransit;
using System;

public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderSubmitted, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentProcessed, x => x.CorrelateById(context => context.Message.OrderId));
        Event(() => PaymentFailed, x => x.CorrelateById(context => context.Message.OrderId));

        Initially(
            When(OrderSubmitted)
                .Then(context =>
                {
                    context.Saga.SubmittedDate = DateTime.UtcNow;
                    context.Saga.Amount = context.Message.Amount;
                    context.Saga.PaymentMethod = context.Message.PaymentMethod;
                })
                .PublishAsync(context => context.Init<ProcessPayment>(new
                {
                    OrderId = context.Saga.CorrelationId,
                    Amount = context.Saga.Amount,
                    PaymentMethod = context.Saga.PaymentMethod
                }))
                .TransitionTo(Submitted)
        );

        During(Submitted,
            When(PaymentProcessed)
                .Then(context =>
                {
                    context.Saga.TransactionId = context.Message.TransactionId;
                    context.Saga.UpdatedDate = DateTime.UtcNow;
                })
                .TransitionTo(Accepted)
                .Finalize(),
            When(PaymentFailed)
                .Then(context =>
                {
                    context.Saga.FailureReason = context.Message.Reason;
                    context.Saga.UpdatedDate = DateTime.UtcNow;
                })
                .TransitionTo(Failed)
        );

        SetCompletedWhenFinalized();
    }

    public State Submitted { get; private set; } = null!;
    public State Accepted { get; private set; } = null!;
    public State Failed { get; private set; } = null!;

    public Event<OrderSubmitted> OrderSubmitted { get; private set; } = null!;
    public Event<PaymentProcessed> PaymentProcessed { get; private set; } = null!;
    public Event<PaymentFailed> PaymentFailed { get; private set; } = null!;
}
