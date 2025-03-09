using MassTransit;

namespace Shared.StockEvents;

public class StockNotReservedEvent : CorrelatedBy<Guid>
{
    public StockNotReservedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }
    public Guid CorrelationId { get; }
    public string Message { get; set; }
}