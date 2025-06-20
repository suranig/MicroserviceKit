# Changelog

All notable changes to MicroserviceKit will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Released] - v0.4.1

### 🐛 Critical Bug Fixes

#### Template Engine & Code Generation
- **FIXED**: Hardcoded entity constructor in ApplicationModule causing compilation errors
  - Replaced rigid constructor signature with flexible pattern and developer guidance
  - Removed incorrect `UpdatedAt = DateTime.UtcNow` during entity creation
  - Added comments for customizing constructor parameters based on domain requirements
- **FIXED**: Fragile template path resolution causing "template not found" errors in packaged CLI
  - Replaced hardcoded relative path with robust multi-strategy discovery
  - Added environment variable override (`MICROSERVICE_TEMPLATES_PATH`)
  - Implemented assembly location search, current directory search, and parent directory traversal
  - Enhanced error handling and fallback mechanisms for different deployment scenarios
- **FIXED**: Dockerfile path and copy errors causing Docker build failures
  - Corrected `WORKDIR` path from `/src/src/Api` to `/src/Api`
  - Implemented conditional COPY statements based on enabled modules and architecture level
  - Prevented "COPY failed: file not found" errors for non-generated projects
  - Enhanced Docker build process with proper project dependency resolution

#### Configuration & Architecture
- **FIXED**: Incorrect configuration property access in DockerModule
  - Updated to use correct architecture level checking instead of non-existent `Features.Application`
  - Improved conditional logic for project inclusion based on architecture patterns

#### Developer Experience
- **ENHANCED**: Generated code now includes helpful comments and guidance
- **IMPROVED**: Error messages and validation feedback throughout generation process
- **STANDARDIZED**: File generation patterns across all template modules

## [Released] - v0.4.0

### 🚀 Major Bug Fixes & Improvements

#### Template Engine Path Resolution
- **FIXED**: Critical path resolution issue where files were generated outside intended output directory
- **FIXED**: Double path combining in `WriteFileAsync()` calls across all modules
- **FIXED**: Template Engine now correctly uses relative paths with proper `src/` and `tests/` prefixes
- **IMPROVED**: Clean project structure generation with everything in correct locations
- **ENHANCED**: File generation increased from ~68 to 90 files including Docker support

#### Code Generation Quality
- **FIXED**: Parametrized aggregate names instead of hardcoded 'Order' references
- **IMPROVED**: Service name to aggregate name conversion (e.g., ProductService → Product)
- **FIXED**: PascalCase property generation in commands and queries
- **FIXED**: String interpolation conflicts in project file templates
- **ENHANCED**: Template placeholder replacement for dynamic content

#### ApplicationModule Enhancements
- **SWITCHED**: From Wolverine to MassTransit for better enterprise stability
- **FIXED**: MassTransit `IConsumer<T>.Consume()` method to return `Task` instead of `Task<Guid>`
- **IMPROVED**: Command/query handler generation with proper parameter handling
- **FIXED**: Validation rules to use correct property names
- **ENHANCED**: Entity construction with proper constructor parameters

#### Docker & Containerization
- **NEW**: Complete DockerModule implementation with proper DI registration
- **ADDED**: Dockerfile, docker-compose.yml, and Makefile generation
- **FIXED**: DockerModule dependencies and project references
- **IMPLEMENTED**: Full containerization support for generated microservices

### 🔧 Module Standardization

#### Path Generation Consistency
- **STANDARDIZED**: All modules now use consistent relative path patterns
- **FIXED**: DDDModule to use `src/Domain/` prefix for all generated files
- **FIXED**: RestApiModule to use `src/Api/` prefix consistently
- **FIXED**: IntegrationTestModule to use `tests/` prefix for test files
- **FIXED**: ReadModelsModule path generation with proper `src/` prefixes
- **FIXED**: ExternalServicesModule to use `src/Infrastructure/` prefix
- **FIXED**: UnitTestModule to use `tests/` prefix instead of absolute paths
- **FIXED**: MessagingModule path issues causing duplicated directory structures

#### File Generation Patterns
- **REPLACED**: All `File.WriteAllTextAsync` calls with `context.WriteFileAsync`
- **REMOVED**: Manual `Directory.CreateDirectory` calls (handled by WriteFileAsync)
- **STANDARDIZED**: Method signatures to include `GenerationContext` parameter
- **IMPROVED**: Error handling and validation in all template modules

### 🏗️ Architecture Improvements

#### MassTransit Integration
- **IMPLEMENTED**: Complete MassTransit setup with RabbitMQ transport
- **ADDED**: Message consumers with proper error handling
- **CONFIGURED**: Dependency injection for messaging infrastructure
- **ENHANCED**: Event-driven architecture support

#### Template Processing
- **FIXED**: Template placeholder replacement in `ApplyCustomizations`
- **IMPROVED**: Configuration validation before code generation
- **ENHANCED**: Template processing pipeline with better error messages
- **STANDARDIZED**: Handlebars template processing across all modules

### 🧪 Testing & Quality

#### Code Quality
- **IMPROVED**: Generated code compilation success rate
- **ENHANCED**: Template validation and error reporting
- **STANDARDIZED**: Naming conventions across all generated files
- **FIXED**: Namespace organization in generated projects

#### Testing Infrastructure
- **MAINTAINED**: Comprehensive test coverage for CLI functionality
- **IMPROVED**: Integration test generation with proper setup
- **ENHANCED**: Test data builders and utilities generation

### 📦 CLI & Developer Experience

#### Command Line Interface
- **MAINTAINED**: All existing CLI functionality working correctly
- **IMPROVED**: Error messages and validation feedback
- **ENHANCED**: Template discovery and application process
- **STANDARDIZED**: Output formatting and progress reporting

#### Configuration Management
- **IMPROVED**: Template configuration processing
- **ENHANCED**: Feature flag handling across modules
- **STANDARDIZED**: Configuration validation patterns

### 🐛 Critical Bug Fixes

#### Path Resolution Issues
- **RESOLVED**: Files no longer generated in main `src/` directory
- **FIXED**: Template Engine respects configured output paths
- **CORRECTED**: Module path calculation logic
- **ELIMINATED**: Double path combining causing incorrect file locations

#### Code Generation Bugs
- **FIXED**: Aggregate name parameterization throughout codebase
- **RESOLVED**: Template placeholder conflicts
- **CORRECTED**: Property naming consistency in generated code
- **FIXED**: Method signature generation for handlers and consumers

#### Module Integration
- **RESOLVED**: Missing DockerModule registration in DI container
- **FIXED**: Module dependency resolution
- **CORRECTED**: Project reference generation
- **STANDARDIZED**: Module interface implementations

### 📋 Documentation & Release

#### Version Management
- **UPDATED**: CLI package version to 0.4.0
- **ENHANCED**: Release notes and changelog documentation
- **IMPROVED**: Version references in documentation

#### Git Workflow
- **IMPLEMENTED**: Thematic git commits for better history
- **CREATED**: Proper git tags for release tracking
- **MAINTAINED**: Clean commit history with logical grouping

### 🚀 Performance & Reliability

#### Generation Performance
- **IMPROVED**: Template processing speed
- **OPTIMIZED**: File I/O operations with proper async patterns
- **ENHANCED**: Memory usage during generation process

#### Error Handling
- **STRENGTHENED**: Exception handling across all modules
- **IMPROVED**: Validation error messages
- **ENHANCED**: Recovery from generation failures

---

## [Released] - v0.3.0

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