# 🚀 .NET 8 Microservice Template Generator

A powerful, configurable template generator for .NET 8 microservices with a modular "Lego blocks" approach. Generate production-ready microservices with Clean Architecture, DDD patterns, CQRS, containerization, and comprehensive testing.

## ✨ Features

### 🏗️ **Architecture Levels**
- **MINIMAL**: Single project for simple CRUD services (1-2 developers)
- **STANDARD**: 3-layer architecture for medium complexity (2-5 developers)  
- **ENTERPRISE**: 4-layer architecture for complex services (5+ developers)
- **AUTO**: Intelligent selection based on your requirements

### 🧩 **Modular Components**
- **Domain Layer**: Aggregates, Entities, Value Objects, Domain Events
- **Application Layer**: Commands, Queries, Handlers, DTOs, Validation
- **API Layer**: REST Controllers with OpenAPI, Validation, Error Handling
- **Infrastructure Layer**: Repositories, DbContext, External Services
- **Testing**: Unit Tests, Integration Tests, Test Utilities

### 🐳 **Containerization**
- **Docker**: Intelligent Dockerfile generation for microservice and infrastructure
- **Docker Compose**: Orchestration with PostgreSQL, MongoDB, RabbitMQ
- **Kubernetes**: Production-ready manifests with HPA, Services, ConfigMaps

### 🔧 **Technologies**
- **.NET 8** with latest C# features
- **Wolverine** for CQRS/Mediator (MIT license)
- **AggregateKit** for DDD base classes
- **Entity Framework Core** for persistence
- **FluentValidation** for input validation
- **xUnit, FluentAssertions, Moq** for testing

## 🚀 Quick Start

### Installation

```bash
# Clone the repository
git clone https://github.com/your-repo/microservice-net8-ddd.git
cd microservice-net8-ddd

# Build the CLI tool
dotnet build src/CLI/MicroserviceGenerator.CLI/MicroserviceGenerator.CLI.csproj

# Install globally (optional)
dotnet pack src/CLI/MicroserviceGenerator.CLI/MicroserviceGenerator.CLI.csproj
dotnet tool install -g MicroserviceGenerator.CLI
```

### Generate Your First Microservice

```bash
# Interactive mode - guided setup
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- new OrderService --interactive

# Quick start with defaults
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- new OrderService

# From configuration file
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- new OrderService --config examples/enterprise-service.json

# Custom output directory
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- new OrderService --output ./my-services/
```

## 📋 Configuration

### Basic Configuration

```json
{
  "ServiceName": "OrderService",
  "RootNamespace": "Company.OrderService",
  "Architecture": {
    "Level": "standard"
  },
  "Features": {
    "Api": {
      "Style": "controllers",
      "Documentation": true,
      "Versioning": true
    },
    "Persistence": {
      "WriteModel": "postgresql",
      "ReadModel": "mongodb"
    },
    "Messaging": {
      "Enabled": true,
      "Provider": "rabbitmq"
    }
  },
  "Domain": {
    "Aggregates": [
      {
        "Name": "Order",
        "Properties": [
          { "Name": "CustomerId", "Type": "Guid" },
          { "Name": "TotalAmount", "Type": "decimal" },
          { "Name": "Status", "Type": "OrderStatus" }
        ],
        "Operations": ["Create", "UpdateStatus", "Cancel"]
      }
    ]
  }
}
```

### Architecture Levels

| Level | Projects | Use Case | Team Size |
|-------|----------|----------|-----------|
| **MINIMAL** | 1 project | Simple CRUD, prototypes | 1-2 devs |
| **STANDARD** | 3 projects | Business logic, medium complexity | 2-5 devs |
| **ENTERPRISE** | 4+ projects | Complex domains, high scalability | 5+ devs |
| **AUTO** | Intelligent | Analyzes your config and decides | Any |

## 🏗️ Generated Structure

### Standard Level
```
OrderService/
├── src/
│   ├── OrderService.Domain/           # Aggregates, Entities, Events
│   ├── OrderService.Application/      # Commands, Queries, Handlers
│   └── OrderService.Api/             # Controllers, DTOs, Middleware
├── tests/
│   ├── OrderService.UnitTests/       # Domain & Application tests
│   └── OrderService.IntegrationTests/ # API & Database tests
├── docker/
│   ├── microservice.Dockerfile       # Main application
│   ├── postgres.Dockerfile          # Write model
│   ├── mongodb.Dockerfile           # Read model
│   └── docker-compose.yml           # Full orchestration
└── k8s/
    ├── deployment.yaml              # Kubernetes deployment
    ├── service.yaml                 # Load balancer
    └── hpa.yaml                     # Auto-scaling
```

### Enterprise Level
```
OrderService/
├── src/
│   ├── OrderService.Domain/          # Rich domain model
│   ├── OrderService.Application/     # Use cases & DTOs
│   ├── OrderService.Infrastructure/  # Persistence & External services
│   └── OrderService.Api/            # REST/gRPC endpoints
├── tests/
│   ├── OrderService.UnitTests/      # Fast, isolated tests
│   ├── OrderService.IntegrationTests/ # Database & API tests
│   └── OrderService.ArchitectureTests/ # Architecture compliance
└── deployment/                      # Full DevOps setup
```

## 🧪 Testing Strategy

### Generated Test Types

**Unit Tests**
- Domain logic and business rules
- Command/Query handlers
- Value object validation
- Domain event handling

**Integration Tests**
- API endpoints with real database
- Repository implementations
- Message publishing/consuming
- External service integration

**Architecture Tests**
- Layer dependency rules
- Naming conventions
- Code quality metrics

### Test Example

```csharp
[Fact]
public async Task CreateOrder_WithValidData_ShouldReturnOrderId()
{
    // Arrange
    var command = new CreateOrderCommand(
        CustomerId: Guid.NewGuid(),
        Items: [new OrderItem("Product", 2, 10.00m)]
    );

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeEmpty();
    _mockRepository.Verify(x => x.AddAsync(
        It.IsAny<Order>(), 
        It.IsAny<CancellationToken>()), 
        Times.Once);
}
```

## 🐳 Docker & Kubernetes

### Docker Compose (Development)

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f orderservice

# Scale microservice
docker-compose up -d --scale orderservice=3
```

### Kubernetes (Production)

```bash
# Deploy to Kubernetes
kubectl apply -f k8s/

# Check status
kubectl get pods -l app=orderservice

# Scale horizontally
kubectl scale deployment orderservice-deployment --replicas=5

# View auto-scaling
kubectl get hpa
```

## 🔄 Migration & Evolution

### Migrate Existing Projects

```bash
# Analyze current project
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- migrate --analyze

# Preview migration to standard level
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- migrate --level standard --dry-run

# Execute migration
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- migrate --level standard

# View migration history
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- history
```

### Smart Auto-Configuration

The generator automatically enables features based on complexity:

- **DDD**: Enabled when complexity >= medium
- **CQRS**: Enabled when multiple operations per aggregate
- **Infrastructure Layer**: Enabled when external dependencies detected
- **Docker**: Enabled for standard+ levels
- **Kubernetes**: Enabled for enterprise level

## 📚 Examples

### Simple CRUD Service
```bash
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- new ProductCatalog --config examples/minimal-crud.json
```

### Event-Driven Microservice
```bash
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- new OrderService --config examples/event-driven.json
```

### Enterprise Service
```bash
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- new PaymentService --config examples/enterprise-service.json
```

## 🛠️ Development

### Prerequisites
- .NET 8 SDK
- Docker & Docker Compose
- Kubernetes (optional)

### Build & Test
```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Run CLI locally
dotnet run --project src/CLI/MicroserviceGenerator.CLI -- --help
```

### Contributing
1. Fork the repository
2. Create feature branch: `git checkout -b feature/amazing-feature`
3. Commit changes: `git commit -m 'Add amazing feature'`
4. Push to branch: `git push origin feature/amazing-feature`
5. Open Pull Request

## 📖 Documentation

- [Development Plan](DEVELOPMENT_PLAN.md) - Detailed roadmap and implementation status
- [Usage Guide](USAGE.md) - Comprehensive usage examples
- [Examples](examples/) - Configuration examples for different scenarios
- [Migration Guide](examples/migration-examples.md) - How to migrate existing projects

## 🎯 Roadmap

### ✅ **Completed (Phase 1-2)**
- [x] Domain Layer generation with AggregateKit
- [x] REST API Controllers with full CRUD
- [x] Comprehensive Unit Testing
- [x] CLI with interactive mode
- [x] Smart architecture level selection

### 🚧 **In Progress (Phase 3)**
- [ ] Application Layer (Commands/Queries/Handlers)
- [ ] Infrastructure Layer (Repositories/DbContext)
- [ ] Integration Testing
- [ ] Docker & Kubernetes support

### 🔮 **Planned (Phase 4+)**
- [ ] gRPC API support
- [ ] Event Sourcing patterns
- [ ] Advanced CQRS with separate read/write models
- [ ] Observability (Logging, Metrics, Tracing)
- [ ] Performance optimization

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Made with ❤️ for .NET developers who want to build better microservices faster.** 
