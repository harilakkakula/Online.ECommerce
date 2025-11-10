using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Common.Messaging.Extensions;
using Common.Messaging.Kafka.Interfaces;
using Common.Messaging.Kafka;
using OrderCreation.EventHandlers;
using Common.Integration;
using OrderCreation.Context;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.IO;
using System;
using FluentValidation;
using FluentValidation.AspNetCore;
using OrderCreation.Business.Validators;
using OrderCreation.Business.Dto;
using OrderCreation.Business.Services.Implementation;
using OrderCreation.Business.Services.Interface;
using Microsoft.EntityFrameworkCore;
using OrderCreation.Business.Data.Repository;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

#region DB
builder.Services.AddDbContext<OrderDbContext>(options => options.UseInMemoryDatabase("OrderServiceDB"));
#endregion

#region Kafka
string bootstrap = builder.Configuration["KAFKA__BOOTSTRAPSERVERS"] ?? "kafka:9092";
builder.Services.AddKafkaMessaging(bootstrap);
builder.Services.AddSingleton<IProducerService>(sp =>
    new ProducerService(bootstrap)
);
#endregion

#region Health check
builder.Services.AddHealthChecks()
.AddKafka(
setup =>
{
   setup.BootstrapServers = bootstrap;
},
name: "kafka",
tags: new[] { "kafka", "message-broker" }
);
#endregion

#region fluent validator
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddScoped<IValidator<OrderCreationDto>, OrderCreationDtoValidator>();
#endregion

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
#region service
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOrderService, OrderService>();
#endregion


#region Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Order Service",
        Version = "v1",
        Description = "Manages orders and publishes order creation events.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Hari Krishna",
            Email = "harilakkakula28@gamil.com"
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

#endregion
#region Kafka Consumer Service
builder.Services.AddScoped<IMessageDispatcher, MessageDispatcher>();
builder.Services.AddScoped<IEventHandler, UserCreatedEventHandler>();
builder.Services.AddHostedService<KafkaConsumerService>();
#endregion



var app = builder.Build();

#region Enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "User Service API V1");
        options.RoutePrefix = string.Empty;
    });
}
#endregion



app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapControllers();
app.Run();

