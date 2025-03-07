using MassTransit;

namespace SagaStateMachine.Service.StateInstances;

public class OrderStateInstance : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public int OrderId { get; set; }
    public int BuyerId { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedDate { get; set; }
}
