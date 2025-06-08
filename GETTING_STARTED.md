# 🚀 Getting Started Guide

This guide will help you generate your first microservice in under 5 minutes.

## Prerequisites

- .NET 8 SDK installed
- Basic understanding of microservices and Clean Architecture

## Step 1: Clone & Build

```bash
git clone https://github.com/your-repo/microservice-net8-ddd.git
cd microservice-net8-ddd
dotnet build
```

## Step 2: Generate Your First Service

### Option A: Interactive Mode (Recommended for beginners)

```bash
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- new MyFirstService --interactive
```

This will guide you through:
- Architecture level selection
- Domain modeling
- Feature selection
- Technology choices

### Option B: Quick Start with Defaults

```bash
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- new MyFirstService
```

This generates a standard-level service with:
- Clean Architecture (Domain + Application + API)
- REST Controllers
- Unit Tests
- Basic CRUD operations

### Option C: From Configuration File

```bash
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- new OrderService --config examples/enterprise-service.json
```

## Step 3: Explore Generated Code

Your service will be generated in `./MyFirstService/` with this structure:

```
MyFirstService/
├── src/
│   ├── MyFirstService.Domain/        # Your business logic
│   ├── MyFirstService.Application/   # Use cases & handlers
│   └── MyFirstService.Api/          # REST endpoints
├── tests/
│   └── MyFirstService.UnitTests/    # Comprehensive tests
└── MyFirstService.sln               # Solution file
```

## Step 4: Run Your Service

```bash
cd MyFirstService
dotnet build
dotnet run --project src/MyFirstService.Api
```

Your API will be available at:
- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:5001
- **Swagger**: https://localhost:5001/swagger

## Step 5: Test Your Service

```bash
# Run all tests
dotnet test

# Test a specific endpoint
curl https://localhost:5001/api/todoitems
```

## What's Generated?

### Domain Layer
- **Aggregates**: Rich domain models with business logic
- **Value Objects**: Immutable data structures
- **Domain Events**: For decoupled communication
- **Specifications**: Business rule validation

### Application Layer
- **Commands**: Write operations (Create, Update, Delete)
- **Queries**: Read operations with filtering and paging
- **Handlers**: Business logic execution
- **DTOs**: Data transfer objects
- **Validators**: Input validation with FluentValidation

### API Layer
- **Controllers**: REST endpoints with full CRUD
- **Middleware**: Error handling, logging, CORS
- **Filters**: Validation and exception handling
- **Documentation**: OpenAPI/Swagger integration

### Testing
- **Unit Tests**: Fast, isolated tests for all layers
- **Test Utilities**: Builders, mocks, and test data
- **Architecture Tests**: Dependency rule validation

## Next Steps

### 1. Customize Your Domain
Edit the generated aggregates in `src/MyFirstService.Domain/Entities/`

### 2. Add Business Logic
Implement your business rules in domain entities and value objects

### 3. Extend API
Add new controllers or modify existing ones in `src/MyFirstService.Api/Controllers/`

### 4. Add Integration Tests
Use the generated test utilities to create integration tests

### 5. Deploy with Docker
```bash
# Generate Docker support
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- new MyFirstService --config examples/docker-enabled.json

# Build and run
docker-compose up -d
```

## Common Patterns

### Adding a New Aggregate

1. **Define the aggregate** in `Domain/Entities/`
2. **Add commands/queries** in `Application/`
3. **Create controller** in `Api/Controllers/`
4. **Write tests** in `Tests/`

### Event-Driven Communication

```csharp
// In your aggregate
public void CompleteOrder()
{
    Status = OrderStatus.Completed;
    AddDomainEvent(new OrderCompletedEvent(Id, CustomerId));
}

// Handler will be auto-generated
public class OrderCompletedEventHandler : INotificationHandler<OrderCompletedEvent>
{
    public async Task Handle(OrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        // Your event handling logic
    }
}
```

### CQRS Pattern

```csharp
// Command (Write)
public record CreateOrderCommand(Guid CustomerId, List<OrderItem> Items);

// Query (Read)  
public record GetOrderByIdQuery(Guid OrderId);

// Handlers are auto-generated with proper validation and error handling
```

## Architecture Levels Explained

| Level | When to Use | Generated Structure |
|-------|-------------|-------------------|
| **MINIMAL** | Simple CRUD, prototypes, learning | Single project with folders |
| **STANDARD** | Most business applications | Domain + Application + API |
| **ENTERPRISE** | Complex domains, large teams | + Infrastructure layer, advanced patterns |
| **AUTO** | Let the generator decide | Analyzes your config and chooses |

## Configuration Examples

### Simple CRUD Service
```json
{
  "ServiceName": "ProductCatalog",
  "Architecture": { "Level": "minimal" },
  "Domain": {
    "Aggregates": [
      {
        "Name": "Product",
        "Properties": [
          { "Name": "Name", "Type": "string" },
          { "Name": "Price", "Type": "decimal" }
        ],
        "Operations": ["Create", "Update", "Delete"]
      }
    ]
  }
}
```

### Event-Driven Service
```json
{
  "ServiceName": "OrderService",
  "Architecture": { "Level": "standard" },
  "Features": {
    "Messaging": { "Enabled": true, "Provider": "rabbitmq" }
  },
  "Domain": {
    "Aggregates": [
      {
        "Name": "Order",
        "Properties": [
          { "Name": "CustomerId", "Type": "Guid" },
          { "Name": "Status", "Type": "OrderStatus" }
        ],
        "Operations": ["Create", "Confirm", "Cancel", "Complete"]
      }
    ]
  }
}
```

## Troubleshooting

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet build
```

### Missing Dependencies
```bash
# Restore packages
dotnet restore
```

### Port Conflicts
Change ports in `appsettings.json` or use:
```bash
dotnet run --project src/MyFirstService.Api --urls "https://localhost:6001;http://localhost:6000"
```

## Need Help?

- 📖 [Full Documentation](README.md)
- 🛠️ [Development Plan](DEVELOPMENT_PLAN.md)
- 💡 [Examples](examples/)
- 🐛 [Report Issues](https://github.com/your-repo/microservice-net8-ddd/issues)

---

**Ready to build amazing microservices? Let's go! 🚀** 