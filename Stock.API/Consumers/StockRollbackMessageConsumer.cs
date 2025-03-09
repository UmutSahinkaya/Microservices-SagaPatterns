using MassTransit;
using Shared.Messages;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class StockRollbackMessageConsumer(MongoDBService mongoDBService) : IConsumer<StockRollbackMessage>
    {
        public Task Consume(ConsumeContext<StockRollbackMessage> context)
        {
            var stockCollection= mongoDBService.GetCollection<Models.Stock>();

        }
    }
}
