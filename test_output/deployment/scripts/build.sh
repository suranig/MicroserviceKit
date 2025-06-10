#!/bin/bash
set -e

echo "Building TestService microservice..."

# Build Docker image
docker build -t testservice-api:latest -f deployment/docker/Dockerfile .

# Tag for registry (update with your registry)
docker tag testservice-api:latest your-registry.com/testservice-api:latest

echo "Build completed successfully!"
echo "To push to registry, run: docker push your-registry.com/testservice-api:latest"