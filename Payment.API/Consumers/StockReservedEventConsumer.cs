using MassTransit;
using Shared.Events;

namespace Payment.API.Consumers
{
    public class StockReservedEventConsumer() : IConsumer<StockReservedEvent>
    {
        public Task Consume(ConsumeContext<StockReservedEvent> context)
        {
            if (true)
            {
                //Ödeme Başarılı...
            }
            else
            {
                //Ödeme Başarısız...
            }
        }
    }
}
