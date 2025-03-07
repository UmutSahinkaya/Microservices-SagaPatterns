using MassTransit;
using MongoDB.Driver;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<PaymentFailedEventConsumer>();
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_PaymentFailedEventQueue, e => e.ConfigureConsumer<PaymentFailedEventConsumer>(context));
    });
});
builder.Services.AddSingleton<MongoDBService>();

var app = builder.Build();

#region Harici- MongoDb'ye SeedData Ekeleme
using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
MongoDBService mongoDbService = scope.ServiceProvider.GetService<MongoDBService>();
var collection = mongoDbService.GetCollection<Stock.API.Models.Stock>();
if (!collection.FindSync(s => true).Any())
{
    await collection.InsertOneAsync(new() { ProductId = Guid.Parse("23849C0F-4FCC-6A45-A99C-BA65354909AF"), Count = 200 });
    await collection.InsertOneAsync(new() { ProductId = Guid.Parse("13D188BB-5553-0D49-9EE4-666D3F0CF8D3"), Count = 100 });
    await collection.InsertOneAsync(new() { ProductId = Guid.Parse("8E607A32-6388-444A-9A5C-28058E0EE836"), Count = 50 });
    await collection.InsertOneAsync(new() { ProductId = Guid.Parse("4F874EB8-6153-C94B-BE8B-9BDA192CEEEB"), Count = 5 });
    await collection.InsertOneAsync(new() { ProductId = Guid.Parse("AEACB7CA-9056-D44C-B590-3C46255F7E33"), Count = 30 });
}
#endregion

app.Run();

