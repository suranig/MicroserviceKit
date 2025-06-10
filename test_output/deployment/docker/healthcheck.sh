#!/bin/bash
set -e

# Health check endpoint
HEALTH_URL="http://localhost:8080/health"

# Perform health check
response=$(curl -s -o /dev/null -w "%{http_code}" $HEALTH_URL)

if [ $response -eq 200 ]; then
    echo "Health check passed"
    exit 0
else
    echo "Health check failed with status code: $response"
    exit 1
fi