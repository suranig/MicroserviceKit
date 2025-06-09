# TestProject

Generated microservice using MicroserviceTemplateGenerator.

## Architecture

This microservice implements:
- Clean Architecture
- Domain-Driven Design (DDD)
- CQRS with Wolverine
- AggregateKit for DDD building blocks

## Getting Started

### Prerequisites
- .NET 8.0 SDK

### Running locally
```bash
dotnet restore
dotnet run --project src/Api/TestProject.Api
```

## API Documentation

The API will be available at: http://localhost:5000/swagger

## Generated Aggregates

- **User**: Email (string), Name (string)
