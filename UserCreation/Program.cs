using Common.Integration;
using Common.Messaging.Kafka.Interfaces;
using Common.Messaging.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Common.Messaging.Extensions;
using UserCreation.Business.Context;
using FluentValidation;
using UserCreation.Business.Validators;
using UserCreation.Business.Dto;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;
using UserCreation.Business.Services.Interface;
using UserCreation.Business.Services.Implementation;
using UserCreation.EventHandlers;
using Microsoft.EntityFrameworkCore;
using UserCreation.Business.Data.Repository;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Prometheus;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        #region Database
        builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseInMemoryDatabase("UserServiceDB"));
        #endregion

        #region Kafka Configuration
        string bootstrapServers = builder.Configuration["KAFKA__BOOTSTRAPSERVERS"] ?? "kafka:9092";
        // Register Kafka-related services
        builder.Services.AddKafkaMessaging(bootstrapServers);

        builder.Services.AddSingleton<IProducerService>(_ =>
            new ProducerService(bootstrapServers));
        #endregion

        #region Health check
        builder.Services.AddHealthChecks()
       .AddKafka(
           setup =>
           {
               setup.BootstrapServers = bootstrapServers;
           },
           name: "kafka",
           tags: new[] { "kafka", "message-broker" }
       );
        #endregion

        #region Repository & Business Services
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        #endregion

        #region Fluent Validation
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddFluentValidationClientsideAdapters();
        builder.Services.AddScoped<IValidator<UserCertationDto>, UserCertationDtoValidator>();
        #endregion

        #region Event Handlers
        builder.Services.AddScoped<IEventHandler, OrderCreatedEventHandler>();
        #endregion

        #region Kafka Consumer Service
        builder.Services.AddScoped<IMessageDispatcher, MessageDispatcher>();
        builder.Services.AddScoped<IEventHandler, OrderCreatedEventHandler>();
        builder.Services.AddHostedService<KafkaConsumerService>();
        #endregion

        #region Swagger Configuration
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "User Service",
                Version = "v1",
                Description = "Manages user accounts and publishes user creation events.",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "Hari Krishna",
                    Email = "harilakkakula28@gmail.com"
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }
        });
        #endregion

        var app = builder.Build();

        #region Middleware & Swagger UI
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

        app.UseRouting();
        // app.UseHttpsRedirection(); // Optional for Docker
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecksUI();

        app.MapMetrics();
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.Run();
    }
}
