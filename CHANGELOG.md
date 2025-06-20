# Changelog

All notable changes to MicroserviceKit will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Released] - v0.4.0 - 2024-12-19

### 🛠️ Critical Template Engine Fixes
- **FIXED**: Template Engine path resolution issues causing files generated outside `src/` directory
- **FIXED**: Directory structure problems - all files now properly placed in `src/` and `tests/`
- **FIXED**: Hardcoded 'Order' aggregate names - now parametrized based on service name
- **FIXED**: Standardized all template modules to use consistent path generation patterns

### 🐳 Complete Docker Support
- **NEW**: DockerModule for comprehensive containerization
- **NEW**: Multi-stage Dockerfile generation for .NET 8 production builds
- **NEW**: docker-compose.yml with PostgreSQL, Redis, RabbitMQ services
- **NEW**: Comprehensive Makefile with build, test, docker, and migration commands
- **NEW**: Production-ready container orchestration

### 📡 Enhanced Messaging & CQRS
- **CHANGED**: Switched from Wolverine to MassTransit for better enterprise messaging stability
- **FIXED**: ApplicationModule CQRS command/query generation
- **FIXED**: Parameter generation to use PascalCase properties
- **FIXED**: MassTransit IConsumer<T>.Consume() method signatures
- **ENHANCED**: Validation rules and entity construction logic

### 🧩 Module Improvements
- **FIXED**: ReadModelsModule path generation and File.WriteAllTextAsync usage
- **FIXED**: ExternalServicesModule to use proper relative paths
- **FIXED**: UnitTestModule and IntegrationTestModule file placement
- **FIXED**: MessagingModule path resolution for events and handlers
- **STANDARDIZED**: All modules now use context.WriteFileAsync with relative paths

### 🏗️ Code Generation Enhancements
- **IMPROVED**: String interpolation handling in generated project files
- **ENHANCED**: Project references and dependency management
- **FIXED**: Template placeholder replacement in aggregate names
- **OPTIMIZED**: Directory creation - now handled automatically by WriteFileAsync

### 📦 Project Structure & Dependencies
- **ADDED**: DockerModule registration in DI container
- **ADDED**: Required project references for Docker module
- **UPDATED**: CLI project file with Docker module dependency
- **FIXED**: Missing using statements and namespace references

## [Unreleased] - v0.3.0

### 📨 **Messaging & Event-Driven Architecture**
- **PLANNED**: `MessagingModule.cs` - Complete event-driven messaging infrastructure
- **PLANNED**: MassTransit integration (primary option) with RabbitMQ transport
- **PLANNED**: Domain events and integration events generation
- **PLANNED**: Publishers and consumers with error handling and retry policies
- **PLANNED**: Docker Compose with RabbitMQ broker
- **PLANNED**: Integration tests for messaging flows
- **PLANNED**: Event sourcing patterns (optional)
- **PLANNED**: Saga patterns for long-running processes

## [Released] - v0.2.0

### 🚀 Major Features Added

#### Phase 4: Template System Reorganization
- **NEW**: `templates/` directory with organized template structure
- **NEW**: `templates/levels/` - Architecture level templates (minimal, standard, enterprise)
- **NEW**: `templates/examples/` - Complete microservice examples
- **NEW**: `templates/configs/` - Base configuration templates
- **NEW**: `templates/README.md` - Comprehensive template documentation
- **NEW**: `templates/index.json` - Template metadata and catalog
- **NEW**: `test_cli/test-templates.sh` - Automated template testing script
- **NEW**: `make cli-test-templates` - Makefile target for template testing
- **MOVED**: All JSON templates from `examples/` to organized `templates/` structure
- **FIXED**: JSON syntax errors (removed invalid comments from template files)
- **ENHANCED**: Template discovery and usage documentation

#### Phase 2: Integration Testing Module
- **NEW**: `IntegrationTestModule.cs` - Complete integration testing infrastructure
- **NEW**: API integration tests with `TestApplicationFactory` and `HttpClient`
- **NEW**: Database integration tests with TestContainers (PostgreSQL, SQL Server)
- **NEW**: End-to-end tests covering full CRUD workflows
- **NEW**: Rate limiting integration tests
- **NEW**: Test infrastructure: `DatabaseFixture`, `TestDataBuilder` with Bogus
- **NEW**: `TestContainersSetup` for containerized database testing
- **NEW**: Test configuration and `GlobalUsings`

#### Phase 3: Containerization & Deployment Module
- **NEW**: `DeploymentModule.cs` - Complete Docker support
- **NEW**: Production-ready Dockerfile with multi-stage build
- **NEW**: Docker Compose with PostgreSQL/SQL Server, Redis
- **NEW**: Security features: resource limits, non-root users
- **NEW**: Deployment scripts: `build.sh`, `deploy.sh`

#### Windows Development Support
- **NEW**: `Dockerfile.windows` for local Windows development
- **NEW**: `docker-compose.windows.yml` - simplified Windows environment
- **NEW**: PowerShell health check script (`healthcheck.ps1`)
- **NEW**: Windows-specific Makefile commands

#### Project Files Generation
- **NEW**: `.gitignore` - comprehensive .NET gitignore
- **NEW**: `.dockerignore` - optimized for Docker builds
- **NEW**: `Makefile` - 179 lines with all development commands
- **NEW**: `VERSION` file for automatic versioning
- **NEW**: `README.md` generation for each microservice

### 🔧 ASP.NET Core API Enhancements

#### Rate Limiting & Performance
- **ENHANCED**: Rate limiting with Fixed Window and Concurrency limiters
- **NEW**: Response compression (Brotli, Gzip)
- **NEW**: Response caching middleware
- **NEW**: Performance monitoring with metrics

#### Health Checks & Monitoring
- **NEW**: Comprehensive health checks (`/health`, `/health/ready`, `/health/live`)
- **NEW**: Database health checks
- **NEW**: Memory and disk space health checks
- **NEW**: Custom health check responses

#### Security Improvements
- **NEW**: Security headers (X-Content-Type-Options, X-Frame-Options, etc.)
- **NEW**: CORS configuration
- **NEW**: JWT authentication enhancements
- **ENHANCED**: Swagger documentation with JWT security definitions

#### API Versioning
- **NEW**: API versioning support (query string, header, URL segment)
- **NEW**: Version-aware Swagger documentation
- **NEW**: Backward compatibility support

### 🐳 Docker & Containerization

#### Docker Images
- **UPDATED**: .NET 8.0.16 with latest security fixes
- **NEW**: Multi-platform support (Linux + Windows)
- **NEW**: Docker image versioning with build arguments
- **NEW**: Optimized layer caching
- **NEW**: Non-root user security (Linux)
- **NEW**: Health checks in containers

#### Container Orchestration
- **NEW**: Production-ready docker-compose.yml
- **NEW**: Development docker-compose.override.yml
- **NEW**: Windows docker-compose.windows.yml (local dev only)
- **NEW**: Environment-specific configurations
- **NEW**: Service discovery and networking

### 📋 Development Experience

#### Makefile Commands
- **NEW**: Version management (`version-patch`, `version-minor`, `version-major`)
- **NEW**: Docker commands (`docker-build`, `docker-run`, `docker-push`)
- **NEW**: Windows-specific commands (`dev-windows`, `docker-build-windows`)
- **NEW**: Development environment (`dev`, `dev-stop`, `dev-logs`)
- **NEW**: Database migrations (`db-migrate`, `db-migration`)
- **NEW**: Release automation (`release-patch`, `release-minor`, `release-major`)
- **NEW**: Health monitoring (`health`, `logs`)

#### Version Management
- **NEW**: Automatic semantic versioning
- **NEW**: Git tagging integration
- **NEW**: Docker image tagging with versions
- **NEW**: Environment variable version injection

### ⚙️ Configuration & Architecture

#### Template Configuration
- **NEW**: `TestingConfiguration` class
- **NEW**: `DeploymentConfiguration` class
- **UPDATED**: Default architecture level from "minimal" to "standard"
- **FIXED**: `ArchitectureRules` to always enable Infrastructure for standard level

#### CLI Improvements
- **NEW**: Registration of IntegrationTestModule
- **NEW**: Registration of DeploymentModule
- **UPDATED**: Default configuration templates
- **ENHANCED**: Template system with organized structure and metadata
- **IMPROVED**: Template validation and error handling
- **FIXED**: CLI test scripts with correct paths and commands

### 🧪 Testing Infrastructure

#### Unit Testing
- **ENHANCED**: Existing unit test generation
- **NEW**: Test builders and utilities
- **NEW**: Domain event testing

#### Integration Testing
- **NEW**: API endpoint testing
- **NEW**: Database integration testing
- **NEW**: Authentication testing
- **NEW**: Rate limiting testing
- **NEW**: Health check testing

#### End-to-End Testing
- **NEW**: Complete workflow testing
- **NEW**: Multi-service testing scenarios
- **NEW**: Performance testing setup

### 🔒 Security & Production Readiness

#### Security Features
- **NEW**: Security headers middleware
- **NEW**: Rate limiting protection
- **NEW**: JWT token validation
- **NEW**: CORS policy configuration
- **NEW**: Container security (non-root, read-only filesystem)

#### Production Features
- **NEW**: Health checks for load balancers
- **NEW**: Graceful shutdown handling
- **NEW**: Resource limits and requests
- **NEW**: Monitoring and alerting setup

### 📊 Monitoring & Observability

#### Logging
- **ENHANCED**: Structured logging configuration
- **NEW**: Request/response logging
- **NEW**: Error tracking and reporting

### 🚧 Infrastructure & Dependencies

#### Package Updates
- **UPDATED**: .NET 8.0.16 (latest with security fixes)
- **NEW**: TestContainers for integration testing
- **NEW**: Bogus for test data generation
- **NEW**: Health check libraries

#### Project Structure
- **NEW**: `src/Modules/Tests/` - Integration testing module
- **NEW**: `src/Modules/Deployment/` - Deployment module
- **UPDATED**: CLI project references

### 🐛 Bug Fixes

#### Core Engine Fixes
- **FIXED**: Missing NuGet dependencies in TemplateEngine.csproj
- **FIXED**: Circular references between modules and abstractions
- **FIXED**: Duplicate ProjectStructureConfiguration class definitions
- **FIXED**: GenerationContext issues with configuration handling
- **FIXED**: MigrationEngine path handling and template application
- **FIXED**: ApiModule implementation of ITemplateModule interface
- **FIXED**: ApplicationModule CQRS file generation
- **FIXED**: Proper namespace organization in all modules
- **FIXED**: DomainModule entity and aggregate generation
- **FIXED**: TemplateEngine orchestration with MicroserviceGenerator
- **FIXED**: CLI Program.cs integration with new TemplateEngine architecture
- **FIXED**: Template discovery and application process

#### Compilation Issues
- **FIXED**: Missing DeploymentConfiguration.Level property references
- **FIXED**: Missing TestingConfiguration in FeaturesConfiguration
- **FIXED**: Deployment module reference in CLI project file
- **FIXED**: Namespace and compilation errors

#### Architecture Issues
- **FIXED**: ArchitectureRules Infrastructure enabling logic
- **FIXED**: Default configuration architecture level
- **FIXED**: Module registration in CLI

### 📚 Documentation

#### Generated Documentation
- **NEW**: Comprehensive README.md for each microservice
- **NEW**: Docker usage documentation
- **NEW**: Development setup instructions

#### Code Documentation
- **ENHANCED**: XML documentation comments
- **NEW**: API documentation with Swagger
- **NEW**: Architecture decision records

## [0.1.0] - 2024-12-XX

### Initial Release
- **NEW**: Core template engine
- **NEW**: CLI tool with interactive mode
- **NEW**: Domain layer with DDD support
- **NEW**: API layer with CRUD controllers
- **NEW**: Unit testing module
- **NEW**: Basic configuration system

---

## Summary of Changes

### Modules Added
1. **IntegrationTestModule** - Complete integration testing infrastructure
2. **DeploymentModule** - Docker and deployment automation

### Files Generated Per Microservice
- **Project Files**: `.gitignore`, `.dockerignore`, `Makefile`, `VERSION`, `README.md`
- **Docker Files**: `Dockerfile`, `Dockerfile.windows`, `docker-compose.yml`, `docker-compose.windows.yml`
- **Scripts**: `build.sh`, `deploy.sh`, health check scripts
- **Tests**: Integration tests, API tests, database tests, end-to-end tests

### Development Commands Available
```bash
# Version management
make version-patch    # 1.0.0 → 1.0.1
make version-minor    # 1.0.0 → 1.1.0
make version-major    # 1.0.0 → 2.0.0

# Development
make dev             # Start Linux environment
make dev-windows     # Start Windows environment
make build           # Build solution
make test            # Run all tests

# Docker
make docker-build           # Build Linux image
make docker-build-windows   # Build Windows image
make docker-run            # Run container

# Release
make release-patch   # Version + Build + Push
make git-tag        # Create git tag
```

### Production Ready Features
- ✅ **Security**: Rate limiting, security headers, JWT auth
- ✅ **Monitoring**: Health checks, structured logging
- ✅ **Reliability**: Graceful shutdown, health probes
- ✅ **Development**: Hot reload, test containers, debugging
- ✅ **CI/CD**: Docker builds, automated deployment, versioning

**Status**: Ready for production use with comprehensive testing and deployment capabilities! 🚀 