# CLI Testing Directory

This directory is used for testing the MicroserviceKit CLI tool. It's excluded from version control via `.gitignore`.

## Directory Structure

```
test_cli/
├── basic/          # Basic microservice generation tests
├── standard/       # Standard architecture tests
├── enterprise/     # Enterprise configuration tests
├── messaging/      # Messaging module tests (future)
└── README.md       # This file
```

## Testing Guidelines

### 1. Before Testing
Always clean the test directory before running new tests:
```bash
rm -rf test_cli/basic/* test_cli/standard/* test_cli/enterprise/*
```

### 2. Basic Testing
Test minimal microservice generation:
```bash
cd test_cli/basic
dotnet run --project ../../src/CLI/CLI.csproj -- generate --name TestBasic --output . --level minimal
```

### 3. Standard Testing
Test standard architecture with all core modules:
```bash
cd test_cli/standard
dotnet run --project ../../src/CLI/CLI.csproj -- generate --name TestStandard --output . --level standard
```

### 4. Enterprise Testing
Test enterprise configuration with all features:
```bash
cd test_cli/enterprise
dotnet run --project ../../src/CLI/CLI.csproj -- generate --name TestEnterprise --output . --level enterprise
```

### 5. Interactive Mode Testing
Test interactive configuration:
```bash
cd test_cli/basic
dotnet run --project ../../src/CLI/CLI.csproj -- generate --interactive
```

### 6. Compilation Testing
After generation, verify the code compiles:
```bash
cd test_cli/standard/TestStandard
dotnet build
```

### 7. Docker Testing
Test Docker build (if DeploymentModule is enabled):
```bash
cd test_cli/standard/TestStandard
docker build -t test-service .
```

## Test Scenarios

### Scenario 1: Basic Microservice
- **Purpose**: Test minimal viable microservice
- **Configuration**: Minimal level, basic features only
- **Expected**: Domain, Application, API layers only

### Scenario 2: Standard Microservice
- **Purpose**: Test standard production-ready microservice
- **Configuration**: Standard level, Infrastructure + Testing
- **Expected**: All core layers + tests + Docker support

### Scenario 3: Enterprise Microservice
- **Purpose**: Test full-featured enterprise microservice
- **Configuration**: Enterprise level, all modules enabled
- **Expected**: Complete microservice with all features

### Scenario 4: Messaging Integration (Future)
- **Purpose**: Test event-driven microservice
- **Configuration**: Standard + Messaging enabled
- **Expected**: Microservice with RabbitMQ integration

## Validation Checklist

After each test, verify:

- [ ] **Generation Success**: CLI completes without errors
- [ ] **File Structure**: All expected files and directories created
- [ ] **Project References**: All .csproj references are valid
- [ ] **Compilation**: `dotnet build` succeeds
- [ ] **Solution File**: .sln includes all projects
- [ ] **Docker Build**: Dockerfile builds successfully (if enabled)
- [ ] **Tests Run**: `dotnet test` passes (if tests enabled)

## Common Issues

### Issue: Project Reference Errors
**Symptom**: Build fails with missing project references
**Solution**: Check if InfrastructureModule is properly implemented

### Issue: Docker Build Fails
**Symptom**: Docker build fails with file not found
**Solution**: Verify Dockerfile paths and .dockerignore configuration

### Issue: Tests Don't Run
**Symptom**: `dotnet test` finds no tests
**Solution**: Check if test projects are included in solution file

## Cleanup Commands

### Clean All Test Directories
```bash
rm -rf test_cli/*/
mkdir -p test_cli/{basic,standard,enterprise,messaging}
```

### Clean Specific Test
```bash
rm -rf test_cli/basic/*
```

### Clean Docker Images
```bash
docker rmi $(docker images -q test-*)
```

## Automation Scripts

### Quick Test Script
```bash
#!/bin/bash
# test_cli/quick-test.sh

echo "🧪 Running CLI Quick Test..."

# Clean
rm -rf test_cli/basic/*

# Generate
cd test_cli/basic
dotnet run --project ../../src/CLI/CLI.csproj -- generate --name QuickTest --output . --level standard

# Build
cd QuickTest
dotnet build

echo "✅ Quick test completed!"
```

### Full Test Suite
```bash
#!/bin/bash
# test_cli/full-test.sh

echo "🧪 Running Full CLI Test Suite..."

# Test all levels
for level in minimal standard enterprise; do
    echo "Testing $level level..."
    rm -rf test_cli/$level/*
    cd test_cli/$level
    dotnet run --project ../../src/CLI/CLI.csproj -- generate --name Test${level^} --output . --level $level
    cd Test${level^}
    dotnet build
    cd ../../..
done

echo "✅ Full test suite completed!"
```

## Notes

- This directory is automatically excluded from Git via `.gitignore`
- Always test in this directory to avoid cluttering the main project
- Clean up after testing to save disk space
- Document any new test scenarios in this README 