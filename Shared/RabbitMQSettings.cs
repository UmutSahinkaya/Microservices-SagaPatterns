namespace Shared;

public static class RabbitMQSettings
{
    public const string StateMachineQueue = "state-machine-queue";
    public const string Stock_OrderCreatedEventQueue = "stock-order-created-event-queue";
    public const string Order_OrderCompletedEventQueue = "order-order-completed-event-queue";
    public const string Order_OrderFailedEventQueue = "order-order-failed-event-queue";
    public const string Stock_RollbackMessageEventQueue = "stock-rollback-message-event-queue";

    public const string Payment_StartedEventQueue = "payment-started-event-queue";
    //public const string Order_PaymentCompletedEventQueue = "order-payment-completed-event-queue";
    //public const string Order_PaymentFailedEventQueue = "order-payment-failed-event-queue";
    //public const string Stock_PaymentFailedEventQueue = "stock-payment-failed-event-queue";
    //public const string Order_StockNotReservedEventQueue = "order-stock-not-reserved-event-queue";
}
