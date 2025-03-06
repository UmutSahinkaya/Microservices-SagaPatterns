using MassTransit;
using Stock.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});
builder.Services.AddSingleton<MongoDBService>();

var app = builder.Build();



app.Run();

