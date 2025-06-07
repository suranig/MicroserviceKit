using MediatR;
using Microservice.Application.Extensions;
using Microservice.Application.Todo.Commands.CreateTodo;
using Microservice.Application.Todo.Queries.GetTodos;
using Microservice.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/todos", async (string title, IMediator mediator, CancellationToken ct) =>
{
    var id = await mediator.Send(new CreateTodoCommand(title), ct);
    return Results.Created($"/todos/{id}", new { Id = id });
});

app.MapGet("/todos", async (IMediator mediator, CancellationToken ct) =>
{
    var items = await mediator.Send(new GetTodosQuery(), ct);
    return Results.Ok(items);
});

app.Run();
