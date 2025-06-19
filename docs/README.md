# 🚀 MicroserviceKit - .NET 8 Microservice Template Generator

[![NuGet Version](https://img.shields.io/nuget/v/MicroserviceKit?style=flat-square&logo=nuget&color=blue)](https://www.nuget.org/packages/MicroserviceKit/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MicroserviceKit?style=flat-square&logo=nuget&color=green)](https://www.nuget.org/packages/MicroserviceKit/)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](https://opensource.org/licenses/MIT)
[![GitHub](https://img.shields.io/badge/GitHub-MicroserviceKit-black?style=flat-square&logo=github)](https://github.com/suranig/MicroserviceKit)

A powerful, configurable template generator for .NET 8 microservices with a modular "Lego blocks" approach. Generate production-ready microservices with Clean Architecture, DDD patterns, CQRS, containerization, and comprehensive testing.

## ✨ Quick Start

```bash
# Install globally
dotnet tool install --global MicroserviceKit --prerelease

# Generate new microservice
microkit new MyService --interactive
```

## 📚 Documentation

- [Getting Started](GETTING_STARTED.md)
- [CLI Reference](docs.md)
- [Architecture Overview](ARCHITECTURE.md)
- [Template Guide](TEMPLATES.md)
- [Testing Guide](TESTING.md)
- [Deployment Guide](DEPLOYMENT.md)
- [Changelog](CHANGELOG.md)

## 🏗️ Architecture Levels

| Level | Projects | Use Case | Team Size |
|-------|----------|----------|-----------|
| **MINIMAL** | 1 project | Simple CRUD, prototypes | 1-2 devs |
| **STANDARD** | 3 projects | Business logic, medium complexity | 2-5 devs |
| **ENTERPRISE** | 4+ projects | Complex domains, high scalability | 5+ devs |

## 🧩 Key Features

- **Domain-Driven Design**: Aggregates, Entities, Value Objects, Domain Events
- **Clean Architecture**: Clear separation of concerns
- **CQRS**: Commands, Queries, Handlers with Wolverine
- **Event-Driven**: Domain events, message brokers
- **Testing**: Unit, Integration, Architecture tests
- **Containerization**: Docker 
- **Infrastructure**: PostgreSQL, MongoDB, Redis, RabbitMQ

## 🚀 Quick Examples

```bash
# Simple CRUD service
microkit new ProductCatalog --level minimal

# Full DDD service
microkit new OrderService --level standard

# Enterprise service
microkit new PaymentService --level enterprise

# Interactive mode
microkit new MyService --interactive
```

## 📦 NuGet Package

**MicroserviceKit** is available on NuGet.org:

- **Package**: [MicroserviceKit](https://www.nuget.org/packages/MicroserviceKit/)
- **Current Version**: 0.3.0
- **Command**: `microkit`
- **Installation**: `dotnet tool install --global MicroserviceKit --prerelease`

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](docs.md#contributing) for details.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

**Made with ❤️ for .NET developers who want to build better microservices faster.**
