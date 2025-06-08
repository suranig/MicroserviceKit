# Templates Directory

This directory contains all template configurations for the MicroserviceKit generator.

## Structure

```
templates/
├── configs/          # Base configuration templates
├── examples/         # Complete example configurations
└── levels/          # Architecture level templates
```

## Configuration Templates (`configs/`)

### `template-config.example.json`
Base template configuration showing all available options with examples.

### `template-config-levels.json`
Configuration template with different architecture levels (minimal, standard, enterprise).

## Architecture Level Templates (`levels/`)

### `minimal-service.json`
- Single project structure
- Basic CRUD operations
- In-memory persistence
- Minimal API style

### `standard-service.json`
- Clean Architecture (Domain, Application, API)
- CQRS patterns
- Database persistence
- Controller-based API

### `enterprise-service.json`
- Full Clean Architecture with Infrastructure layer
- Advanced patterns (DDD, CQRS, Event Sourcing)
- External services integration
- Messaging and events
- Docker and Kubernetes support

## Complete Examples (`examples/`)

### `complete-microservice.json`
Full-featured microservice configuration demonstrating all available features.

## Usage

### Using a level template:
```bash
dotnet run --project src/CLI/MicroserviceGenerator.CLI/MicroserviceGenerator.CLI.csproj -- new MyService --config templates/levels/standard-service.json --output ./output
```

### Using a complete example:
```bash
dotnet run --project src/CLI/MicroserviceGenerator.CLI/MicroserviceGenerator.CLI.csproj -- new MyService --config templates/examples/complete-microservice.json --output ./output
```

### Interactive mode with base config:
```bash
dotnet run --project src/CLI/MicroserviceGenerator.CLI/MicroserviceGenerator.CLI.csproj -- new MyService --interactive --config templates/configs/template-config.example.json --output ./output
```

## Template Development

When creating new templates:

1. **Start with a base config** from `configs/`
2. **Customize for specific use case**
3. **Test with the generator**
4. **Add to appropriate category**

### Template Validation

All templates should be valid JSON and follow the `TemplateConfiguration` schema defined in:
`src/Core/TemplateEngine/Configuration/TemplateConfiguration.cs`

## Architecture Decisions

Templates are organized by:
- **Complexity level** (minimal → standard → enterprise)
- **Use case** (examples for specific scenarios)
- **Configuration type** (base configs for customization)

This structure allows users to:
- Quickly find templates for their architecture level
- Use complete examples as starting points
- Customize base configurations for specific needs 