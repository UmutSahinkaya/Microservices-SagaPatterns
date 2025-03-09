using MassTransit;
using MongoDB.Driver;
using Shared;
using Stock.API.Consumers;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<OrderCreatedEventConsumer>();
    configurator.AddConsumer<StockRollbackMessageConsumer>();
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_OrderCreatedEventQueue, e => e.ConfigureConsumer<OrderCreatedEventConsumer>(context));
        _configure.ReceiveEndpoint(RabbitMQSettings.Stock_RollbackMessageQueue, e => e.ConfigureConsumer<StockRollbackMessageConsumer>(context));

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
    await collection.InsertOneAsync(new() { ProductId = 1, Count = 100 });
    await collection.InsertOneAsync(new() { ProductId = 2, Count = 200 });
    await collection.InsertOneAsync(new() { ProductId = 3, Count = 30 });
    await collection.InsertOneAsync(new() { ProductId = 4, Count = 40 });
    await collection.InsertOneAsync(new() { ProductId = 5, Count = 5 });
}
#endregion

app.Run();

