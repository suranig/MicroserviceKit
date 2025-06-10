# TestService Makefile
# Version management and common tasks

# Variables
SERVICE_NAME := testservice
VERSION := $(shell cat VERSION 2>/dev/null || echo "1.0.0")
DOCKER_REGISTRY := localhost:5000
DOCKER_IMAGE := $(DOCKER_REGISTRY)/$(SERVICE_NAME)
DOCKER_TAG := $(VERSION)
NAMESPACE := $(SERVICE_NAME)

# Colors for output
RED := \033[0;31m
GREEN := \033[0;32m
YELLOW := \033[1;33m
BLUE := \033[0;34m
NC := \033[0m # No Color

.PHONY: help version build test clean docker-build docker-push deploy k8s-deploy

# Default target
help: ## Show this help message
	@echo "$(BLUE)TestService - Available commands:$(NC)"
	@awk 'BEGIN {FS = ":.*?## "} /^[a-zA-Z_-]+:.*?## / {printf "  $(GREEN)%-15s$(NC) %s\n", $$1, $$2}' $(MAKEFILE_LIST)

# Version management
version: ## Show current version
	@echo "Current version: $(YELLOW)$(VERSION)$(NC)"

version-patch: ## Bump patch version (1.0.0 -> 1.0.1)
	@echo "$(YELLOW)Bumping patch version...$(NC)"
	@echo $(VERSION) | awk -F. '{$$3++; print $$1"."$$2"."$$3}' > VERSION
	@echo "New version: $(GREEN)$$(cat VERSION)$(NC)"

version-minor: ## Bump minor version (1.0.0 -> 1.1.0)
	@echo "$(YELLOW)Bumping minor version...$(NC)"
	@echo $(VERSION) | awk -F. '{$$2++; $$3=0; print $$1"."$$2"."$$3}' > VERSION
	@echo "New version: $(GREEN)$$(cat VERSION)$(NC)"

version-major: ## Bump major version (1.0.0 -> 2.0.0)
	@echo "$(YELLOW)Bumping major version...$(NC)"
	@echo $(VERSION) | awk -F. '{$$1++; $$2=0; $$3=0; print $$1"."$$2"."$$3}' > VERSION
	@echo "New version: $(GREEN)$$(cat VERSION)$(NC)"

# Build and test
restore: ## Restore NuGet packages
	@echo "$(BLUE)Restoring packages...$(NC)"
	dotnet restore

build: restore ## Build the solution
	@echo "$(BLUE)Building solution...$(NC)"
	dotnet build --configuration Release --no-restore

test: build ## Run all tests
	@echo "$(BLUE)Running tests...$(NC)"
	dotnet test --configuration Release --no-build --verbosity normal

test-coverage: ## Run tests with coverage
	@echo "$(BLUE)Running tests with coverage...$(NC)"
	dotnet test --configuration Release --collect:"XPlat Code Coverage" --results-directory ./TestResults

clean: ## Clean build artifacts
	@echo "$(BLUE)Cleaning...$(NC)"
	dotnet clean
	rm -rf **/bin **/obj TestResults/

# Docker commands
docker-build: ## Build Docker image (Linux)
	@echo "$(BLUE)Building Docker image...$(NC)"
	docker build -f deployment/docker/Dockerfile -t $(DOCKER_IMAGE):$(DOCKER_TAG) -t $(DOCKER_IMAGE):latest --build-arg VERSION=$(VERSION) .

docker-build-windows: ## Build Docker image (Windows)
	@echo "$(BLUE)Building Windows Docker image...$(NC)"
	docker build -f deployment/docker/Dockerfile.windows -t $(DOCKER_IMAGE):$(DOCKER_TAG)-windows --build-arg VERSION=$(VERSION) .

docker-push: docker-build ## Push Docker image to registry
	@echo "$(BLUE)Pushing Docker image...$(NC)"
	docker push $(DOCKER_IMAGE):$(DOCKER_TAG)
	docker push $(DOCKER_IMAGE):latest

docker-run: ## Run Docker container locally (Linux)
	@echo "$(BLUE)Running Docker container...$(NC)"
	docker run -d -p 8080:8080 --name $(SERVICE_NAME) $(DOCKER_IMAGE):latest

docker-run-windows: ## Run Docker container locally (Windows)
	@echo "$(BLUE)Running Windows Docker container...$(NC)"
	docker run -d -p 8080:8080 --name $(SERVICE_NAME)-windows $(DOCKER_IMAGE):$(DOCKER_TAG)-windows

docker-stop: ## Stop Docker container
	@echo "$(BLUE)Stopping Docker container...$(NC)"
	docker stop $(SERVICE_NAME) || true
	docker rm $(SERVICE_NAME) || true
	docker stop $(SERVICE_NAME)-windows || true
	docker rm $(SERVICE_NAME)-windows || true

docker-compose-up: ## Start with docker-compose
	@echo "$(BLUE)Starting with docker-compose...$(NC)"
	cd deployment/docker && docker-compose up -d

docker-compose-down: ## Stop docker-compose
	@echo "$(BLUE)Stopping docker-compose...$(NC)"
	cd deployment/docker && docker-compose down

# Kubernetes commands
k8s-deploy: ## Deploy to Kubernetes
	@echo "$(BLUE)Deploying to Kubernetes...$(NC)"
	kubectl apply -f deployment/kubernetes/

k8s-delete: ## Delete from Kubernetes
	@echo "$(BLUE)Deleting from Kubernetes...$(NC)"
	kubectl delete -f deployment/kubernetes/

k8s-status: ## Show Kubernetes status
	@echo "$(BLUE)Kubernetes status:$(NC)"
	kubectl get pods,services,ingress -n $(NAMESPACE)

k8s-logs: ## Show Kubernetes logs
	@echo "$(BLUE)Kubernetes logs:$(NC)"
	kubectl logs -f deployment/$(SERVICE_NAME)-api -n $(NAMESPACE)

# Development commands
dev: ## Start development environment (Linux)
	@echo "$(BLUE)Starting development environment...$(NC)"
	cd deployment/docker && docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

dev-windows: ## Start development environment (Windows)
	@echo "$(BLUE)Starting Windows development environment...$(NC)"
	cd deployment/docker && docker-compose -f docker-compose.windows.yml up -d

dev-logs: ## Show development logs
	@echo "$(BLUE)Development logs:$(NC)"
	cd deployment/docker && docker-compose logs -f

dev-stop: ## Stop development environment
	@echo "$(BLUE)Stopping development environment...$(NC)"
	cd deployment/docker && docker-compose down

dev-stop-windows: ## Stop Windows development environment
	@echo "$(BLUE)Stopping Windows development environment...$(NC)"
	cd deployment/docker && docker-compose -f docker-compose.windows.yml down

# Database commands
db-migrate: ## Run database migrations
	@echo "$(BLUE)Running database migrations...$(NC)"
	dotnet ef database update --project src/Infrastructure/TestService.Infrastructure

db-migration: ## Create new migration (usage: make db-migration NAME=MigrationName)
	@echo "$(BLUE)Creating migration: $(NAME)$(NC)"
	dotnet ef migrations add $(NAME) --project src/Infrastructure/TestService.Infrastructure

# Release commands
release-patch: version-patch docker-build docker-push ## Release patch version
	@echo "$(GREEN)Released patch version: $$(cat VERSION)$(NC)"

release-minor: version-minor docker-build docker-push ## Release minor version
	@echo "$(GREEN)Released minor version: $$(cat VERSION)$(NC)"

release-major: version-major docker-build docker-push ## Release major version
	@echo "$(GREEN)Released major version: $$(cat VERSION)$(NC)"

# Git commands
git-tag: ## Create git tag with current version
	@echo "$(BLUE)Creating git tag: v$(VERSION)$(NC)"
	git tag -a v$(VERSION) -m "Release version $(VERSION)"
	git push origin v$(VERSION)

# Health check
health: ## Check service health
	@echo "$(BLUE)Checking service health...$(NC)"
	curl -f http://localhost:8080/health || echo "$(RED)Service is not healthy$(NC)"

# Monitoring
logs: ## Show application logs
	@echo "$(BLUE)Application logs:$(NC)"
	docker logs -f $(SERVICE_NAME) 2>/dev/null || echo "$(YELLOW)Container not running$(NC)"

metrics: ## Show metrics endpoint
	@echo "$(BLUE)Metrics endpoint:$(NC)"
	curl -s http://localhost:8080/metrics || echo "$(RED)Metrics not available$(NC)"