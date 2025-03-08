using MassTransit;
using SagaStateMachine.Service.StateInstances;
using Shared;
using Shared.Messages;
using Shared.OrderEvents;
using Shared.PaymentEvents;
using Shared.StockEvents;

namespace SagaStateMachine.Service.StateMachines;

public class OrderStateMachine:MassTransitStateMachine<OrderStateInstance>
{
    public Event<OrderStartedEvent> OrderStartedEvent { get; set; }
    public Event<StockReservedEvent> StockReservedEvent { get; set; }
    public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
    public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }
    public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }


    public State OrderCreated { get; set; }
    public State StockReserved { get; set; }
    public State StockNotReserved { get; set; }
    public State PaymentCompleted { get; set; }
    public State PaymentFailed { get; set; }

    public OrderStateMachine()
    {
        InstanceState(instance => instance.CurrentState);
        //Eğer gelen event tetikleyici event se order ıd verisiyle veritabanında uyuşan veri varsa corrletionId oluşturma onu kullan yoksa oluştur.
        Event(() => OrderStartedEvent,
            orderStateInstance => orderStateInstance.CorrelateBy<int>(database =>
            database.OrderId, @event => @event.Message.OrderId)
            .SelectId(e => Guid.NewGuid()));

        Event(() => StockReservedEvent,
            orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));
        Event(() => StockNotReservedEvent,
            orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));
        Event(() => PaymentCompletedEvent,
            orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));
        Event(() => PaymentFailedEvent,
            orderStateInstance => orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

        // etap1 son video son 1 saatte anlatılıyor14.ADIM
        Initially(
            When(OrderStartedEvent)
         .Then(context =>
         {
             context.Saga.OrderId = context.Message.OrderId;
             context.Saga.BuyerId = context.Message.BuyerId;
             context.Saga.TotalPrice = context.Message.TotalPrice;
             context.Saga.CreatedDate = DateTime.UtcNow;
         })
         .TransitionTo(OrderCreated)
         .Send(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEventQueue}"),
         context=>new OrderCreatedEvent(context.Saga.CorrelationId)
         {
             OrderItems=context.Message.OrderItems
         }));

        During(OrderCreated,
            When(StockReservedEvent)
            .TransitionTo(StockReserved)
            .Send(new Uri($"queue:{RabbitMQSettings.Payment_StartedEventQueue}"),
            context=>new PaymentStartedEvent(context.Saga.CorrelationId)
            {
                TotalPrice=context.Saga.TotalPrice,
                OrderItems=context.Message.OrderItems,
            }),
            When(StockNotReservedEvent)
            .TransitionTo(StockNotReserved)
            .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
            context => new OrderFailedEvent
            {
                OrderId=context.Saga.OrderId,
                Message=context.Message.Message
            }));

        During(StockReserved,
            When(PaymentCompletedEvent)
            .TransitionTo(PaymentCompleted)
            .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderCompletedEventQueue}"),
            context => new OrderCompletedEvent
            {
                OrderId=context.Saga.OrderId
            })
            .Finalize(),
             When(PaymentFailedEvent)
            .TransitionTo(PaymentFailed)
            .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
            context => new StockRollbackMessage
            {
                OrderItems=context.Message.OrderItems
            }));

        SetCompletedWhenFinalized();
    }
}
