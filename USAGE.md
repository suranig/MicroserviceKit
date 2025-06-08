# MicroserviceTemplateGenerator - Usage Guide

Cookiecutter-like template generator for .NET 8 microservices.

## Installation

```bash
# Install as global tool
dotnet tool install -g MicroserviceTemplateGenerator

# Or install locally
dotnet tool install MicroserviceTemplateGenerator
```

## Quick Start

### 1. Generate new microservice (interactive mode)

```bash
dotnet microservice new OrderService --interactive
```

This will prompt you for:
- Namespace (default: Company.OrderService)
- DDD patterns (aggregates, value objects)
- API type (REST, gRPC, both)
- Persistence (in-memory, PostgreSQL, SQL Server)

### 2. Generate with default template

```bash
dotnet microservice new TodoService
```

Creates a simple microservice with:
- Clean Architecture structure
- One "Item" aggregate with Title and IsCompleted properties
- REST API endpoints
- In-memory persistence
- Wolverine for CQRS
- AggregateKit for DDD patterns

### 3. Generate from configuration file

```bash
dotnet microservice new OrderService --config order-config.json
```

Example `order-config.json`:
```json
{
  "microserviceName": "OrderService",
  "namespace": "ECommerce.OrderService",
  "ddd": {
    "enabled": true,
    "aggregates": [
      {
        "name": "Order",
        "properties": [
          { "name": "CustomerId", "type": "Guid" },
          { "name": "TotalAmount", "type": "decimal" },
          { "name": "Status", "type": "OrderStatus" }
        ],
        "methods": ["AddItem", "RemoveItem", "Confirm"]
      }
    ],
    "valueObjects": [
      {
        "name": "Money",
        "properties": [
          { "name": "Amount", "type": "decimal" },
          { "name": "Currency", "type": "string" }
        ]
      }
    ]
  },
  "api": {
    "types": ["rest", "grpc"]
  },
  "persistence": {
    "writeModel": "postgresql"
  }
}
```

## Commands

### Generate new microservice

```bash
dotnet microservice new <name> [options]

Options:
  --config <file>     Configuration file path
  --output <dir>      Output directory (default: ./)
  --interactive       Interactive mode
```

### Add components to existing microservice

```bash
# Add new aggregate
dotnet microservice add aggregate Customer --properties "Email:string,Name:string"

# Add value object
dotnet microservice add valueobject Address --properties "Street:string,City:string"

# Add API endpoint
dotnet microservice add api orders --type rest --operations "create,read,update,delete"
```

## Generated Structure

```
OrderService/
├── OrderService.sln
├── README.md
├── src/
│   ├── Api/
│   │   └── OrderService.Api/
│   │       ├── Controllers/
│   │       ├── Program.cs
│   │       └── OrderService.Api.csproj
│   ├── Application/
│   │   └── OrderService.Application/
│   │       ├── Commands/
│   │       ├── Queries/
│   │       ├── Extensions/
│   │       └── OrderService.Application.csproj
│   ├── Domain/
│   │   └── OrderService.Domain/
│   │       ├── Entities/
│   │       ├── Events/
│   │       ├── ValueObjects/
│   │       └── OrderService.Domain.csproj
│   └── Infrastructure/
│       └── OrderService.Infrastructure/
│           ├── Repositories/
│           ├── Extensions/
│           └── OrderService.Infrastructure.csproj
└── tests/
    └── OrderService.Tests/
```

## Features

### ✅ Implemented
- Interactive CLI like cookiecutter
- DDD patterns with AggregateKit
- CQRS with Wolverine (open source)
- Clean Architecture structure
- Configurable aggregates and value objects
- REST API generation
- Project structure generation

### 🚧 Coming Soon
- gRPC service generation
- Database persistence modules
- Docker/Kubernetes support
- Background workers
- Messaging (RabbitMQ, Azure Service Bus)
- Event sourcing module

## Examples

### E-commerce Order Service

```bash
dotnet microservice new OrderService --interactive
```

When prompted:
- Namespace: `ECommerce.OrderService`
- Enable DDD: `Yes`
- Aggregates: 
  - `Order` with properties: `CustomerId:Guid`, `TotalAmount:decimal`
  - `Customer` with properties: `Email:string`, `Name:string`
- API type: `rest`
- Persistence: `postgresql`

### Simple Todo Service

```bash
dotnet microservice new TodoService
```

Uses default configuration with simple Item aggregate.

### Blog Service

```bash
dotnet microservice new BlogService --config blog-config.json
```

With `blog-config.json`:
```json
{
  "namespace": "Blog.Service",
  "ddd": {
    "aggregates": [
      {
        "name": "Article",
        "properties": [
          { "name": "Title", "type": "string" },
          { "name": "Content", "type": "string" },
          { "name": "AuthorId", "type": "Guid" },
          { "name": "PublishedAt", "type": "DateTime?" }
        ],
        "methods": ["Publish", "Unpublish", "UpdateContent"]
      }
    ]
  }
}
```

## Next Steps After Generation

1. Navigate to generated directory:
   ```bash
   cd OrderService
   ```

2. Restore packages:
   ```bash
   dotnet restore
   ```

3. Run the service:
   ```bash
   dotnet run --project src/Api/OrderService.Api
   ```

4. Open Swagger UI:
   ```
   http://localhost:5000/swagger
   ```

## Tips

- Use `--interactive` for first-time setup to understand all options
- Save frequently used configurations as JSON files
- Generated code follows .NET 8 best practices
- All dependencies are open source (no commercial licenses)
- Generated projects are ready to run immediately 