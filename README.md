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
- **⚡ CQRS Pattern** - Command Query Responsibility Segregation with MassTransit
- **🔄 Event-Driven Architecture** - RabbitMQ, MassTransit, Domain Events
- **🗄️ Multi-Database Support** - PostgreSQL (write), MongoDB (read), Redis (cache)
- **🔗 External Services Integration** - HTTP clients with Polly resilience
- **🧪 Comprehensive Testing** - Unit, Integration, E2E with TestContainers
- **🐳 Docker & Kubernetes** - Production-ready containerization with Dockerfile, docker-compose.yml, Makefile
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
# Interactive mode (recommended for beginners)
microkit generate MyService --interactive

# Quick start with enterprise template
microkit generate ArticleService --template cqrs-event-sourcing

# With custom aggregates and external services
microkit generate MyService --template cqrs-event-sourcing --aggregates Article Comment --external-services ImageService VideoService
```

### Explore available templates

```bash
# List all templates
microkit list

# List by category
microkit list --category service-types

# Get detailed template information
microkit describe cqrs-event-sourcing.json
```

## 📋 Available Templates

### 🏷️ Enterprise Service Types
Perfect for production microservices with complex business requirements:

#### **`cqrs-event-sourcing.json`** ⭐ Most Popular
- **Use for**: Services with complex business rules, audit trail requirements, event-driven architecture
- **Complexity**: Enterprise (25 minutes)
- **Features**: CQRS, Event Sourcing, Domain Events, Aggregate Root, PostgreSQL
- **Perfect for**: Article management, Order processing, Financial transactions

#### **`bpc-workflow.json`** 
- **Use for**: Business process orchestration, workflow management, external system integration
- **Complexity**: Enterprise (30 minutes)
- **Features**: Workflow Engine, Saga Pattern, External Integrations, State Machine, Hangfire
- **Perfect for**: Payment processing, Document approval workflows, Multi-step business processes

#### **`event-store.json`**
- **Use for**: Centralized event storage, event replay, audit logs
- **Complexity**: Enterprise (20 minutes)
- **Features**: Event Store, Event Sourcing, Projections, Event Replay
- **Perfect for**: Event sourcing infrastructure, Audit systems, Event analytics

#### **`read-model.json`**
- **Use for**: High-performance read operations, reporting, data projections
- **Complexity**: Medium (15 minutes)
- **Features**: CQRS Read Side, MongoDB, Projections, Redis Cache, Event Handlers
- **Perfect for**: Reporting services, Search APIs, Data analytics

#### **`identity-auth.json`**
- **Use for**: User authentication, authorization, multi-tenant security
- **Complexity**: Enterprise (25 minutes)
- **Features**: OAuth2, JWT, RBAC, Multi-Auth, Identity Server
- **Perfect for**: Identity provider, User management, API security

#### **`api-gateway.json`**
- **Use for**: Centralized API management, routing, cross-cutting concerns
- **Complexity**: Enterprise (20 minutes)
- **Features**: API Gateway, Routing, Rate Limiting, Circuit Breaker, Ocelot
- **Perfect for**: Microservices entry point, API aggregation, Security gateway

#### **`multi-tenant.json`**
- **Use for**: SaaS applications, tenant isolation, configuration per tenant
- **Complexity**: Enterprise (20 minutes)
- **Features**: Multi-tenancy, Tenant Isolation, Configuration per Tenant, Domain Routing
- **Perfect for**: SaaS platforms, Multi-client systems, Tenant-specific features

#### **`legacy-adapter.json`**
- **Use for**: Integration with legacy systems, data synchronization
- **Complexity**: Medium (15 minutes)
- **Features**: Adapter Pattern, Legacy Integration, Data Sync, Anti-corruption Layer
- **Perfect for**: Legacy system integration, Data migration, System modernization

#### **`search-indexing.json`**
- **Use for**: Full-text search, content indexing, search analytics
- **Complexity**: Medium (15 minutes)
- **Features**: Elasticsearch, Search Indexing, Event-driven, Full-text Search
- **Perfect for**: Content search, Product catalogs, Document search

#### **`cqrs-simple.json`**
- **Use for**: CQRS without Event Sourcing, simpler business logic
- **Complexity**: Medium (15 minutes)
- **Features**: CQRS, Clean Architecture, PostgreSQL, Redis Cache
- **Perfect for**: CRUD-heavy services, Simple business logic, Standard applications

### 🏗️ Architecture Levels
Choose based on your project complexity:

#### **`minimal-service.json`** 🚀 Quick Start
- **Use for**: Prototypes, simple CRUD operations, learning
- **Complexity**: Simple (5 minutes)
- **Features**: Minimal API, In-memory DB, Basic CRUD
- **Perfect for**: MVPs, Proof of concepts, Simple utilities

#### **`standard-service.json`** 📊 Balanced
- **Use for**: Standard business applications, moderate complexity
- **Complexity**: Medium (10 minutes)
- **Features**: Clean Architecture, CQRS, SQLite, JWT Auth
- **Perfect for**: Business applications, Standard APIs, Moderate complexity

#### **`enterprise-service.json`** 🏢 Full Featured
- **Use for**: Enterprise applications, all patterns included
- **Complexity**: Complex (20 minutes)
- **Features**: DDD, CQRS, Event Sourcing, Microservices Patterns, Docker, Kubernetes
- **Perfect for**: Large enterprises, Complex domains, Full-scale microservices

## 🏗️ Generated Architecture

Every generated service includes:

```
MyService/
├── src/
│   ├── Domain/                 # Domain layer (DDD)
│   │   ├── Aggregates/         # Aggregate roots with business logic
│   │   ├── Entities/           # Domain entities
│   │   ├── ValueObjects/       # Immutable value objects
│   │   ├── Events/             # Domain events
│   │   └── Repositories/       # Repository interfaces
│   ├── Application/            # Application layer (CQRS)
│   │   ├── Commands/           # Command handlers (write operations)
│   │   ├── Queries/            # Query handlers (read operations)
│   │   ├── DTOs/               # Data transfer objects
│   │   └── Behaviors/          # Cross-cutting concerns
│   ├── Infrastructure/         # Infrastructure layer
│   │   ├── Persistence/        # EF Core, MongoDB, Redis
│   │   ├── ExternalServices/   # HTTP clients with Polly
│   │   ├── Messaging/          # MassTransit + RabbitMQ
│   │   └── Configuration/      # Dependency injection
│   └── Api/                    # API layer
│       ├── Controllers/        # REST API controllers
│       ├── Middleware/         # Custom middleware
│       └── Extensions/         # Service registration
├── tests/
│   ├── Unit/                   # Unit tests with xUnit
│   ├── Integration/            # Integration tests with TestContainers
│   └── EndToEnd/               # E2E tests
├── Dockerfile                  # Multi-stage Docker build
├── docker-compose.yml          # Complete development environment
├── Makefile                    # Development commands
├── k8s/                        # Kubernetes manifests
└── docs/                       # Auto-generated documentation
```

## 🛠️ CLI Commands

### 🚀 Generate Commands

```bash
# Interactive mode (recommended)
microkit generate MyService --interactive

# Quick generation with template
microkit generate ArticleService --template cqrs-event-sourcing

# Custom aggregates
microkit generate MyService --template cqrs-event-sourcing --aggregates Article Comment User

# External services integration
microkit generate MyService --template cqrs-event-sourcing --external-services ImageService VideoService NotificationService

# Full customization
microkit generate MyService --template cqrs-event-sourcing \
  --aggregates Article Comment \
  --external-services ImageService VideoService \
  --database postgresql \
  --output ./services/ArticleService
```

### 📋 Discovery Commands

```bash
# List all templates
microkit list

# List by category
microkit list --category service-types
microkit list --category levels

# Filter by complexity
microkit list --complexity enterprise
microkit list --complexity medium

# Detailed view with descriptions
microkit list --detailed

# Get specific template information
microkit describe cqrs-event-sourcing.json
microkit describe api-gateway.json --format json
```

## 🔧 Template Customization

### Example: Article Service Configuration

```json
{
  "templateType": "cqrs-event-sourcing",
  "microserviceName": "ArticleService",
  "namespace": "Company.ArticleService",
  "domain": {
    "aggregates": [
      {
        "name": "Article",
        "properties": [
          { "name": "Title", "type": "string", "isRequired": true },
          { "name": "Content", "type": "string", "isRequired": true },
          { "name": "Status", "type": "ArticleStatus", "isRequired": true }
        ],
        "operations": ["Create", "Update", "Publish", "Archive"]
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
      "services": [
        {
          "name": "ImageService",
          "baseUrl": "https://api.images.com",
          "operations": ["Upload", "Process", "Delete"]
        }
      ]
    }
  },
  "deployment": {
    "docker": { "enabled": true },
    "kubernetes": { "enabled": true }
  }
}
```

## 🧪 Testing & Development

### Generated Development Environment

Every service includes a complete development setup:

```bash
# Start development environment
make dev-setup

# Run the service
make run

# Run tests
make test

# Build Docker image
make docker-build

# Run with Docker
make docker-run

# Clean up
make clean
```

### Testing with TestContainers

```bash
# Unit tests
dotnet test tests/Unit/

# Integration tests with real databases
dotnet test tests/Integration/

# End-to-end tests
dotnet test tests/EndToEnd/

# All tests
make test
```

## 🐳 Docker & Kubernetes

### Complete Containerization

Every generated service includes:

- **Multi-stage Dockerfile** - Optimized for production
- **docker-compose.yml** - Complete development environment with PostgreSQL, MongoDB, Redis, RabbitMQ
- **Kubernetes manifests** - Production-ready K8s deployment
- **Health checks** - Liveness and readiness probes
- **Makefile** - All development commands

```bash
# Development with Docker Compose
docker-compose up -d

# Build production image
docker build -t myservice:latest .

# Deploy to Kubernetes
kubectl apply -f k8s/
```

## 📊 Monitoring & Observability

### Built-in Monitoring

- **Health Checks** - `/health`, `/health/ready`, `/health/live`
- **Metrics** - ASP.NET Core built-in metrics
- **Structured Logging** - Serilog with correlation IDs
- **Distributed Tracing** - OpenTelemetry ready

```bash
# Health endpoints
curl http://localhost:5000/health
curl http://localhost:5000/health/ready
curl http://localhost:5000/health/live

# Metrics endpoint
curl http://localhost:5000/metrics
```

## 🔐 Security

### Authentication & Authorization

```bash
# JWT for internal services
curl -H "Authorization: Bearer <jwt-token>" \
     http://localhost:5000/api/articles

# OAuth2 for external applications
curl -H "Authorization: Bearer <oauth-token>" \
     http://localhost:5000/api/articles
```

## 🌟 Real-World Examples

### Headless CMS Architecture

```bash
# Article management service
microkit generate ArticleService --template cqrs-event-sourcing --aggregates Article ArticleBlock

# Page management service
microkit generate PageService --template cqrs-event-sourcing --aggregates Page PageLayout

# Content read service
microkit generate ContentReadService --template read-model --aggregates ArticleProjection PageProjection

# Search service
microkit generate SearchService --template search-indexing

# Identity service
microkit generate IdentityService --template identity-auth --aggregates User Role Permission

# API Gateway
microkit generate ApiGateway --template api-gateway
```

### E-commerce Platform

```bash
# Order management
microkit generate OrderService --template cqrs-event-sourcing --aggregates Order OrderItem

# Product catalog
microkit generate ProductService --template cqrs-simple --aggregates Product Category

# Inventory management
microkit generate InventoryService --template cqrs-event-sourcing --aggregates Inventory

# Payment processing
microkit generate PaymentService --template bpc-workflow --aggregates Payment PaymentMethod

# Search service
microkit generate SearchService --template search-indexing
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
