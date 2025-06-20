using Microsoft.Extensions.Logging;
using MassTransit;

namespace OrderService.Application.Middleware;

public class LoggingFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly ILogger<LoggingFilter<T>> _logger;

    public LoggingFilter(ILogger<LoggingFilter<T>> logger)
    {
        _logger = logger;
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var messageType = typeof(T).Name;
        
        _logger.LogInformation("Processing message {MessageType}", messageType);
        
        try
        {
            await next.Send(context);
            _logger.LogInformation("Successfully processed message {MessageType}", messageType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message {MessageType}", messageType);
            throw;
        }
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("logging");
    }
}