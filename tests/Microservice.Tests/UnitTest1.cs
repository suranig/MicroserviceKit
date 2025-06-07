using Microservice.Application.Extensions;
using Microservice.Application.Todo.Commands.CreateTodo;
using Microservice.Application.Todo.Queries.GetTodos;
using Microservice.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Microservice.Tests;

public class TodoTests
{
    [Fact]
    public async Task CreatingTodoReturnsId()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddInfrastructure();
        using var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<MediatR.IMediator>();
        var id = await mediator.Send(new CreateTodoCommand("test"));
        var todos = await mediator.Send(new GetTodosQuery());
        Assert.Contains(todos, x => x.Id == id);
    }
}
