# 🚀 MicroserviceKit - .NET 8 Microservice Template Generator

[![NuGet Version](https://img.shields.io/nuget/v/MicroserviceKit?style=flat-square&logo=nuget&color=blue)](https://www.nuget.org/packages/MicroserviceKit/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MicroserviceKit?style=flat-square&logo=nuget&color=green)](https://www.nuget.org/packages/MicroserviceKit/)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)
[![GitHub](https://img.shields.io/badge/GitHub-MicroserviceKit-black?style=flat-square&logo=github)](https://github.com/suranig/MicroserviceKit)

🚀 **Complete toolkit for generating production-ready .NET 8 microservices with DDD, CQRS, Clean Architecture, Event-Driven Architecture, and comprehensive infrastructure.**

## ✨ Features

- **🏗️ Clean Architecture** - Domain, Application, Infrastructure, API layers
- **📦 Domain-Driven Design** - Aggregates, Entities, Value Objects, Domain Events
- **⚡ CQRS Pattern** - Command Query Responsibility Segregation with Wolverine
- **🔄 Event-Driven Architecture** - RabbitMQ, MassTransit, Domain Events
- **🗄️ Multi-Database Support** - PostgreSQL (write), MongoDB (read), Redis (cache)
- **🔗 External Services Integration** - HTTP clients with Polly resilience
- **🧪 Comprehensive Testing** - Unit, Integration, E2E with TestContainers
- **🐳 Docker & Kubernetes** - Production-ready containerization
- **📊 Monitoring & Health Checks** - Built-in observability
- **🔐 Security** - JWT authentication, OAuth2, RBAC
- **🌐 API Gateway** - Centralized routing and authentication

## 🚀 Quick Start

### Install the CLI tool

```bash
dotnet tool install -g MicroserviceKit
```

### Generate your first microservice

```bash
# Interactive mode
microkit generate MyService --interactive

# Using a template
microkit generate ArticleService --template article-service

# With customizations
microkit generate MyService --template cqrs-event-sourcing --aggregates Article Comment --external-services ImageService
```

### List available templates

```bash
microkit list templates
microkit list templates --category service-types
microkit describe article-service
```

## 📋 Available Templates

### Service Types
- **`article-service`** - CQRS + Event Sourcing for content management
- **`tag-taxonomy`** - Flat taxonomy for tags and labels
- **`category-taxonomy`** - Hierarchical taxonomy for categories
- **`identity-service`** - Authentication and authorization (JWT + OAuth)
- **`tenant-service`** - Multi-tenancy management
- **`bpc-service`** - Business Process Control with external providers
- **`legacy-sync`** - Legacy system integration

### Architecture Levels
- **`minimal`** - Single project, basic CRUD
- **`standard`** - Clean Architecture, CQRS
- **`enterprise`** - Full DDD, Event Sourcing, External Services

## 🏗️ Generated Architecture

```
MyService/
├── src/
│   ├── Domain/                 # Domain layer
│   │   ├── Aggregates/         # Aggregate roots
│   │   ├── Entities/           # Domain entities
│   │   ├── ValueObjects/       # Value objects
│   │   ├── Events/             # Domain events
│   │   └── Repositories/       # Repository interfaces
│   ├── Application/            # Application layer
│   │   ├── Commands/           # Command handlers
│   │   ├── Queries/            # Query handlers
│   │   ├── DTOs/               # Data transfer objects
│   │   └── Behaviors/          # Cross-cutting concerns
│   ├── Infrastructure/         # Infrastructure layer
│   │   ├── Persistence/        # Database context & repositories
│   │   ├── ExternalServices/   # HTTP clients
│   │   ├── Messaging/          # Event publishing
│   │   └── Configuration/      # App configuration
│   └── Api/                    # API layer
│       ├── Controllers/        # REST controllers
│       ├── Middleware/         # Custom middleware
│       └── Extensions/         # Service registration
├── tests/
│   ├── Unit/                   # Unit tests
│   ├── Integration/            # Integration tests
│   └── EndToEnd/               # E2E tests
├── docker/                     # Docker configuration
├── k8s/                        # Kubernetes manifests
└── docs/                       # Documentation
```

## 🔧 Configuration

### Template Customization

```json
{
  "templateType": "article-service",
  "microserviceName": "ArticleService",
  "namespace": "Company.ArticleService",
  "domain": {
    "aggregates": [
      {
        "name": "Article",
        "properties": [
          { "name": "Title", "type": "string", "isRequired": true },
          { "name": "Content", "type": "string", "isRequired": true }
        ],
        "operations": ["Create", "Update", "Publish"]
      }
    ]
  },
  "features": {
    "database": {
      "writeModel": { "provider": "postgresql" },
      "readModel": { "provider": "mongodb" }
    },
    "messaging": { "enabled": true, "provider": "rabbitmq" },
    "externalServices": {
      "enabled": true,
      "services": ["ImageService", "VideoService"]
    }
  }
}
```

## 🛠️ CLI Commands

### Generate Commands
```bash
# Basic generation
microkit generate MyService --template article-service

# Interactive mode
microkit generate MyService --interactive

# Custom aggregates
microkit generate MyService --template article-service --aggregates Article Comment User

# External services
microkit generate MyService --template article-service --external-services ImageService VideoService

# Database and messaging
microkit generate MyService --template article-service --database postgresql --messaging rabbitmq
```

### List Commands
```bash
# List all templates
microkit list templates

# Filter by category
microkit list templates --category service-types

# Filter by complexity
microkit list templates --complexity enterprise

# Detailed view
microkit list templates --detailed
```

### Describe Commands
```bash
# Template details
microkit describe article-service

# Different formats
microkit describe article-service --format json
microkit describe article-service --format markdown
```

### History Commands
```bash
# Show generation history
microkit history

# Different formats
microkit history --format json
microkit history --format summary
```

## 🧪 Testing

### Run Tests
```bash
# Unit tests
dotnet test tests/Unit/

# Integration tests
dotnet test tests/Integration/

# All tests
dotnet test
```

### Test with TestContainers
```bash
# Start test containers
docker-compose -f tests/docker-compose.test.yml up -d

# Run integration tests
dotnet test tests/Integration/ --filter Category=Integration
```

## 🐳 Docker & Kubernetes

### Build and Run
```bash
# Build image
docker build -t myservice:latest .

# Run with Docker Compose
docker-compose up -d

# Deploy to Kubernetes
kubectl apply -f k8s/
```

## 📊 Monitoring

### Health Checks
```bash
# Health endpoint
curl http://localhost:5000/health

# Readiness check
curl http://localhost:5000/health/ready

# Liveness check
curl http://localhost:5000/health/live
```

### Metrics
```bash
# Prometheus metrics
curl http://localhost:5000/metrics
```

## 🔐 Security

### Authentication
```bash
# JWT for internal users
curl -H "Authorization: Bearer <jwt-token>" http://localhost:5000/api/articles

# OAuth2 for external apps
curl -H "Authorization: Bearer <oauth-token>" http://localhost:5000/api/articles
```

## 📚 Documentation

- [Getting Started Guide](docs/GETTING_STARTED.md)
- [Architecture Overview](docs/ARCHITECTURE.md)
- [CLI Reference](docs/CLI_REFERENCE.md)
- [Template Guide](docs/TEMPLATES.md)
- [Testing Guide](docs/TESTING.md)
- [Deployment Guide](docs/DEPLOYMENT.md)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- 📖 [Documentation](docs/)
- 🐛 [Issues](https://github.com/suranig/microservice-net8-ddd/issues)
- 💬 [Discussions](https://github.com/suranig/microservice-net8-ddd/discussions)

---

**Built with ❤️ for the .NET community**
