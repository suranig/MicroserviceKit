using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TestService.Infrastructure.Messaging.Configuration;

public class RabbitMQConfiguration
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string VirtualHost { get; set; } = "/";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "testservice.events";
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
}

public static class RabbitMQServiceExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMQConfig = configuration.GetSection("RabbitMQ").Get<RabbitMQConfiguration>() 
            ?? new RabbitMQConfiguration();

        services.AddSingleton(rabbitMQConfig);

        services.AddSingleton<IConnectionFactory>(provider =>
        {
            var config = provider.GetRequiredService<RabbitMQConfiguration>();
            return new ConnectionFactory
            {
                HostName = config.HostName,
                Port = config.Port,
                VirtualHost = config.VirtualHost,
                UserName = config.UserName,
                Password = config.Password,
                DispatchConsumersAsync = true
            };
        });

        services.AddSingleton<IConnection>(provider =>
        {
            var factory = provider.GetRequiredService<IConnectionFactory>();
            var logger = provider.GetRequiredService<ILogger<IConnection>>();
            
            try
            {
                var connection = factory.CreateConnection("TestService");
                logger.LogInformation("Connected to RabbitMQ");
                return connection;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to connect to RabbitMQ");
                throw;
            }
        });

        services.AddScoped<IModel>(provider =>
        {
            var connection = provider.GetRequiredService<IConnection>();
            var channel = connection.CreateModel();
            
            var config = provider.GetRequiredService<RabbitMQConfiguration>();
            
            // Declare exchange
            channel.ExchangeDeclare(
                exchange: config.ExchangeName,
                type: ExchangeType.Topic,
                durable: config.Durable,
                autoDelete: config.AutoDelete);
                
            return channel;
        });

        return services;
    }
}