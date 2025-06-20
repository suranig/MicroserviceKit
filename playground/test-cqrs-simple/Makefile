.PHONY: build run test clean docker-build docker-run docker-stop

# Build the application
build:
	dotnet build

# Run the application
run:
	dotnet run --project src/Api/SimpleService.Api.csproj

# Run tests
test:
	dotnet test

# Clean build artifacts
clean:
	dotnet clean
	rm -rf bin obj

# Docker commands
docker-build:
	docker-compose build

docker-run:
	docker-compose up -d

docker-stop:
	docker-compose down

docker-logs:
	docker-compose logs -f

# Development setup
dev-setup:
	docker-compose up -d postgres redis rabbitmq

dev-stop:
	docker-compose down

# Database migrations
migrate:
	dotnet ef database update --project src/Infrastructure/SimpleService.Infrastructure.csproj --startup-project src/Api/SimpleService.Api.csproj

# Generate migration
migration:
	@read -p "Enter migration name: " name; \
	dotnet ef migrations add $$name --project src/Infrastructure/SimpleService.Infrastructure.csproj --startup-project src/Api/SimpleService.Api.csproj