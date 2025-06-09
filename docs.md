# 📚 MicroserviceKit Documentation

## Table of Contents
1. [Getting Started](#getting-started)
2. [Configuration Guide](#configuration-guide)
3. [Usage Examples](#usage-examples)
4. [Architecture Levels](#architecture-levels)
5. [Features](#features)
6. [Migration Guide](#migration-guide)
7. [FAQ](#faq)

## Getting Started

### Installation
```bash
# Install globally from NuGet
dotnet tool install --global MicroserviceKit --prerelease

# Or install specific version
dotnet tool install --global MicroserviceKit --version 0.2.0
```

### Basic Usage
```bash
# Show help
microkit --help

# List available commands
microkit list-commands

# Show version
microkit --version
```

## Configuration Guide

### 1. Interactive Mode
The easiest way to start is using interactive mode:
```bash
microkit new MyService --interactive
```

This will guide you through:
- Architecture level selection
- Domain model configuration
- API style selection
- Database configuration
- Testing setup
- Deployment options

### 2. Configuration File
For more control, use a JSON configuration file:

```json
{
  "microserviceName": "OrderService",
  "architecture": {
    "level": "standard",
    "patterns": {
      "ddd": "enabled",
      "cqrs": "enabled"
    }
  },
  "domain": {
    "aggregates": [
      {
        "name": "Order",
        "properties": [
          { "name": "CustomerId", "type": "Guid" },
          { "name": "TotalAmount", "type": "decimal" }
        ],
        "operations": ["Create", "UpdateStatus"]
      }
    ]
  },
  "features": {
    "api": {
      "style": "controllers",
      "authentication": "jwt"
    },
    "database": {
      "writeModel": "postgresql",
      "readModel": "mongodb",
      "cache": "redis"
    }
  }
}
```

### 3. Quick Level Selection
For simple cases, just specify the architecture level:
```bash
microkit new MyService --level minimal    # Simple CRUD
microkit new MyService --level standard   # Full DDD
microkit new MyService --level enterprise # Enterprise features
```

## Usage Examples

### 1. Simple Blog Service
```bash
# Interactive mode
microkit new BlogService --interactive

# Or with config
microkit new BlogService --config templates/examples/blog-service.json
```

### 2. E-commerce Order Service
```bash
# With full DDD and CQRS
microkit new OrderService --config templates/examples/e-commerce-order.json
```

### 3. Banking Payment Service
```bash
# Enterprise level with all features
microkit new PaymentService --config templates/examples/banking-payment.json
```

## Architecture Levels

### Minimal Level
- Single project structure
- Basic CRUD operations
- Simple controllers
- In-memory database
- Basic unit tests

### Standard Level
- 3-layer architecture (Domain, Application, API)
- Full DDD implementation
- CQRS with Wolverine
- PostgreSQL for write model
- MongoDB for read model
- Redis for caching
- Integration tests
- Docker support

### Enterprise Level
- 4-layer architecture (Domain, Application, Infrastructure, API)
- All Standard features
- Event-driven architecture
- External services integration
- Advanced deployment
- Kubernetes support
- Comprehensive testing suite

## Features

### Domain Layer
- Aggregates with AggregateKit
- Domain Events
- Value Objects
- Domain Services
- Validation Rules

### Application Layer
- Commands and Queries
- Command/Query Handlers
- DTOs
- Validation
- Authorization

### API Layer
- REST Controllers
- OpenAPI/Swagger
- API Versioning
- Error Handling
- Authentication/Authorization

### Infrastructure Layer
- Repositories
- DbContext
- External Services
- Message Brokers
- Caching

### Testing
- Unit Tests
- Integration Tests
- Test Containers
- Architecture Tests

### Deployment
- Docker
- Docker Compose
- Kubernetes
- Health Checks
- Monitoring

## Migration Guide

### Analyzing Current Project
```bash
# Analyze current project structure
microkit migrate --analyze

# Show migration history
microkit history
```

### Migration Process
```bash
# Preview migration to standard level
microkit migrate --level standard --dry-run

# Execute migration
microkit migrate --level standard

# Force migration (if needed)
microkit migrate --level standard --force
```

### Migration History
```bash
# View migration history
microkit history

# View detailed history
microkit history --format json

# View with snapshots
microkit history --snapshots
```

## FAQ

### Q: Which mode should I use?
- Use `--interactive` for guided setup
- Use `--config` for repeatable configurations
- Use `--level` for quick starts

### Q: How to customize generated code?
- Use custom templates in `templates/custom/`
- Override specific files in `templates/overrides/`
- Use post-generation hooks

### Q: How to add new aggregates to existing service?
```bash
# Add new aggregate
microkit add aggregate Order --properties "Id:Guid,Total:decimal"

# Add with operations
microkit add aggregate Order --operations "Create,Update,Cancel"
```

### Q: How to test the generated service?
```bash
# Run unit tests
dotnet test

# Run integration tests
dotnet test --filter "Category=Integration"

# Run all tests
dotnet test --filter "Category!=Integration"
```

### Q: How to deploy the service?
```bash
# Development with Docker Compose
docker-compose up -d

# Production with Kubernetes
kubectl apply -f k8s/
```

### Q: How to update the CLI tool?
```bash
# Update to latest version
dotnet tool update --global MicroserviceKit

# Update to specific version
dotnet tool update --global MicroserviceKit --version 0.2.0
```

## Contributing

### Development Setup
```bash
# Clone repository
git clone https://github.com/suranig/microservice-net8-ddd.git

# Build solution
make build

# Run tests
make test

# Run CLI tests
make cli-test
```

### Adding New Features
1. Fork the repository
2. Create feature branch
3. Implement changes
4. Add tests
5. Update documentation
6. Submit pull request

## Support

- GitHub Issues: [Report bugs or request features](https://github.com/suranig/microservice-net8-ddd/issues)
- Documentation: [Full documentation](https://github.com/suranig/microservice-net8-ddd/docs)
- Examples: [Example configurations](https://github.com/suranig/microservice-net8-ddd/templates/examples) 