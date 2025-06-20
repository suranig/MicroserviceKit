using FluentValidation;
using MassTransit;

namespace TestService.Application.Middleware;

public class ValidationFilter<T> : IFilter<ConsumeContext<T>>
    where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        var validationResult = await _validator.ValidateAsync(context.Message);
        
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        await next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("validation");
    }
}