# 🔄 Migration Examples

## Overview

The Migration Engine allows seamless upgrades between architecture levels without manual code rewriting.

## 🎯 **Migration Scenarios**

### 1. **MINIMAL → STANDARD**
```bash
# Current: Single project with folders
# Target: 3 separate projects (Domain, Application, Api)

dotnet microservice migrate --level standard --dry-run
```

**What happens:**
- ✅ Creates `Domain`, `Application`, `Api` projects
- ✅ Moves domain entities to `Domain` project
- ✅ Moves commands/queries to `Application` project
- ✅ Moves controllers to `Api` project
- ✅ Updates project references
- ✅ Updates namespaces
- ✅ Updates solution file
- ✅ Removes old single project

### 2. **STANDARD → ENTERPRISE**
```bash
# Current: 3 projects (Domain, Application, Api)
# Target: 4+ projects with Infrastructure

dotnet microservice migrate --level enterprise --dry-run
```

**What happens:**
- ✅ Creates `Infrastructure` project
- ✅ Moves repositories to `Infrastructure/Persistence`
- ✅ Generates read model repositories
- ✅ Adds external service clients
- ✅ Configures resilience patterns
- ✅ Updates dependency injection

### 3. **Configuration-Based Migration**
```bash
# Use specific configuration file
dotnet microservice migrate --config enterprise-order-service.json --dry-run
```

## 📋 **Migration Plan Example**

```
🔄 Analyzing project at: /src/OrderService

📊 Analyzing migration requirements...

📋 Migration Plan:
   From: Minimal (1 projects)
   To:   Standard (3 projects)
   Duration: ~5 minutes

📝 Steps:
   01. 🔄 Create Domain project at src/OrderService.Domain
   02. 🔄 Create Application project at src/OrderService.Application
   03. 🔄 Create Api project at src/OrderService.Api
   04. 🔄 Move Domain/*.cs from src/OrderService/Domain/ to src/OrderService.Domain/
   05. 🔄 Move Application/*.cs from src/OrderService/Application/ to src/OrderService.Application/
   06. 🔄 Move Controllers/*.cs from src/OrderService/Controllers/ to src/OrderService.Api/Controllers/
   07. 🔄 Move Program.cs from src/OrderService/ to src/OrderService.Api/
   08. 🔄 Update project references
   09. 🔄 Update namespaces
   10. 🔄 Update solution file
   11. ⚠️  Delete old project at src/OrderService

⚠️  Warnings:
   • Step 11 is not reversible - backup recommended
   • Ensure all changes are committed to git before migration

Do you want to proceed with this migration? (y/N):
```

## 🛡️ **Safety Features**

### **Dry Run Mode**
```bash
# See what would happen without executing
dotnet microservice migrate --level enterprise --dry-run
```

### **Automatic Rollback**
```bash
# If migration fails, automatic rollback occurs
dotnet microservice migrate --level standard
# ❌ Migration failed: File access denied
# 🔄 Rollback completed automatically.
```

### **Force Mode**
```bash
# Override safety checks (use with caution)
dotnet microservice migrate --level enterprise --force
```

## 🔄 **Real Migration Examples**

### **Example 1: Simple CRUD → Standard Architecture**

**Before (MINIMAL):**
```
src/
└── OrderService/
    ├── Program.cs
    ├── Controllers/
    │   └── OrdersController.cs
    ├── Domain/
    │   └── Order.cs
    └── Application/
        ├── CreateOrderCommand.cs
        └── GetOrdersQuery.cs
```

**After (STANDARD):**
```
src/
├── OrderService.Domain/
│   ├── OrderService.Domain.csproj
│   └── Entities/
│       └── Order.cs
├── OrderService.Application/
│   ├── OrderService.Application.csproj
│   ├── Commands/
│   │   └── CreateOrderCommand.cs
│   └── Queries/
│       └── GetOrdersQuery.cs
└── OrderService.Api/
    ├── OrderService.Api.csproj
    ├── Program.cs
    └── Controllers/
        └── OrdersController.cs
```

### **Example 2: Standard → Enterprise with External Services**

**Migration Command:**
```bash
dotnet microservice migrate --config enterprise-config.json
```

**enterprise-config.json:**
```json
{
  "serviceName": "OrderService",
  "architecture": {
    "level": "enterprise"
  },
  "features": {
    "persistence": {
      "writeModel": {
        "provider": "PostgreSQL",
        "enableMigrations": true,
        "enableAuditing": true
      },
      "readModel": {
        "provider": "MongoDB",
        "enableProjections": true
      }
    },
    "externalServices": [
      {
        "name": "PaymentService",
        "type": "HTTP",
        "baseUrl": "https://api.payments.com"
      }
    ],
    "resilience": {
      "enableRetry": true,
      "enableCircuitBreaker": true
    }
  }
}
```

**Generated Infrastructure:**
```
src/
├── OrderService.Infrastructure/
│   ├── OrderService.Infrastructure.csproj
│   ├── Persistence/
│   │   ├── Write/
│   │   │   ├── OrderDbContext.cs
│   │   │   └── OrderRepository.cs
│   │   └── Read/
│   │       ├── OrderReadModelRepository.cs
│   │       └── OrderProjectionHandler.cs
│   ├── ExternalServices/
│   │   └── PaymentServiceClient.cs
│   └── Resilience/
│       └── ResiliencePolicies.cs
```

## 🎛️ **Interactive Migration**

```bash
dotnet microservice migrate --interactive
```

```
🎯 Interactive Migration Configuration

Current architecture: Minimal (1 project)
Target architecture level (minimal/standard/enterprise): standard

📊 Detected aggregates: Order, Customer
📊 Detected commands: CreateOrder, UpdateOrder, CreateCustomer
📊 Detected queries: GetOrders, GetCustomer

Enable Infrastructure layer? (y/N): n
Enable external services? (y/N): n
Enable caching? (y/N): n

📋 Migration Plan:
   From: Minimal → Standard
   Duration: ~3 minutes
   Steps: 10

Proceed? (y/N): y
```

## 🔧 **Advanced Migration Features**

### **Selective Migration**
```bash
# Migrate only specific components
dotnet microservice migrate --level standard --components domain,application
```

### **Backup Integration**
```bash
# Automatic backup before migration
dotnet microservice migrate --level enterprise --backup
```

### **Git Integration**
```bash
# Create git branch for migration
dotnet microservice migrate --level standard --git-branch migration/to-standard
```

## 📊 **Migration Metrics**

After successful migration:
```
✅ Migration completed successfully!

⏱️  Duration: 47.3 seconds
📁 Generated files: 12
📝 Modified files: 3

📁 Generated files:
   + src/OrderService.Domain/OrderService.Domain.csproj
   + src/OrderService.Application/OrderService.Application.csproj
   + src/OrderService.Api/OrderService.Api.csproj
   + src/OrderService.Domain/Entities/Order.cs
   + src/OrderService.Application/Commands/CreateOrderCommand.cs
   ... and 7 more
```

## 🚨 **Migration Best Practices**

1. **Always use `--dry-run` first**
2. **Commit all changes to git before migration**
3. **Test the application after migration**
4. **Review generated code for customizations**
5. **Update CI/CD pipelines if needed**

## 🔄 **Rollback Strategy**

While automatic rollback handles failures, for manual rollback:

```bash
# If you need to manually rollback
git checkout HEAD~1  # Go back to pre-migration state
```

The migration system makes architectural evolution **painless and safe**! 🎉 