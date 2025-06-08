# MicroserviceKit Development Makefile
# Provides convenient commands for development, testing, and building

.PHONY: help build test clean cli-test cli-test-quick cli-test-full cli-clean dev-setup

# Default target
help:
	@echo "🛠️  MicroserviceKit Development Commands"
	@echo "========================================"
	@echo ""
	@echo "📦 Build Commands:"
	@echo "  build          - Build the entire solution"
	@echo "  clean          - Clean build artifacts"
	@echo "  restore        - Restore NuGet packages"
	@echo ""
	@echo "🧪 Testing Commands:"
	@echo "  test           - Run unit tests"
	@echo "  cli-test       - Run CLI quick test"
	@echo "  cli-test-full  - Run full CLI test suite"
	@echo "  cli-clean      - Clean CLI test directories"
	@echo ""
	@echo "🚀 Development Commands:"
	@echo "  dev-setup      - Setup development environment"
	@echo "  format         - Format code"
	@echo "  lint           - Run code analysis"
	@echo ""
	@echo "📦 Package Commands:"
	@echo "  pack           - Create NuGet packages"
	@echo "  publish        - Publish to NuGet (requires API key)"
	@echo ""

# Build commands
build:
	@echo "🔨 Building solution..."
	dotnet build

clean:
	@echo "🧹 Cleaning build artifacts..."
	dotnet clean
	rm -rf **/bin **/obj

restore:
	@echo "📦 Restoring NuGet packages..."
	dotnet restore

# Testing commands
test:
	@echo "🧪 Running unit tests..."
	dotnet test

cli-test:
	@echo "🧪 Running CLI quick test..."
	./test_cli/quick-test.sh

cli-test-full:
	@echo "🧪 Running full CLI test suite..."
	./test_cli/full-test.sh

cli-clean:
	@echo "🧹 Cleaning CLI test directories..."
	./test_cli/clean.sh

# Development commands
dev-setup:
	@echo "🚀 Setting up development environment..."
	dotnet restore
	@echo "✅ Development environment ready!"

format:
	@echo "🎨 Formatting code..."
	dotnet format

lint:
	@echo "🔍 Running code analysis..."
	dotnet build --verbosity normal

# Package commands
pack:
	@echo "📦 Creating NuGet packages..."
	dotnet pack --configuration Release --output ./nupkg

publish:
	@echo "🚀 Publishing to NuGet..."
	@echo "⚠️  Make sure you have set NUGET_API_KEY environment variable"
	dotnet nuget push "./nupkg/*.nupkg" --source https://api.nuget.org/v3/index.json --api-key $(NUGET_API_KEY)

# CLI testing shortcuts
test-basic:
	@echo "🧪 Testing basic microservice generation..."
	cd test_cli/basic && rm -rf * && \
	dotnet run --project ../../src/CLI/CLI.csproj -- generate --name TestBasic --output . --level minimal && \
	cd TestBasic && dotnet build

test-standard:
	@echo "🧪 Testing standard microservice generation..."
	cd test_cli/standard && rm -rf * && \
	dotnet run --project ../../src/CLI/CLI.csproj -- generate --name TestStandard --output . --level standard && \
	cd TestStandard && dotnet build

test-enterprise:
	@echo "🧪 Testing enterprise microservice generation..."
	cd test_cli/enterprise && rm -rf * && \
	dotnet run --project ../../src/CLI/CLI.csproj -- generate --name TestEnterprise --output . --level enterprise && \
	cd TestEnterprise && dotnet build

# Interactive CLI testing
cli-interactive:
	@echo "🎮 Running CLI in interactive mode..."
	cd test_cli/basic && \
	dotnet run --project ../../src/CLI/CLI.csproj -- generate --interactive

# Development workflow
dev: clean restore build test
	@echo "✅ Development workflow completed!"

# CI/CD workflow
ci: clean restore build test cli-test
	@echo "✅ CI/CD workflow completed!"

# Show project status
status:
	@echo "📊 Project Status"
	@echo "=================="
	@echo "Solution file: $(shell ls *.sln)"
	@echo "Projects: $(shell find src -name "*.csproj" | wc -l)"
	@echo "Test projects: $(shell find src -name "*Test*.csproj" | wc -l)"
	@echo "CLI test directories: $(shell ls -d test_cli/*/ 2>/dev/null | wc -l)"
	@echo ""
	@echo "Recent CLI tests:"
	@ls -la test_cli/*/Test* 2>/dev/null | head -5 || echo "No recent tests found" 