#!/bin/bash

# Cleanup script for CLI test directories
# Removes all generated test projects and Docker images

echo "🧹 Cleaning CLI test directories..."
echo "==================================="

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "📁 Test directory: $SCRIPT_DIR"

# Clean all test subdirectories
echo "🗑️  Removing generated projects..."
rm -rf "$SCRIPT_DIR/basic"/*
rm -rf "$SCRIPT_DIR/standard"/*
rm -rf "$SCRIPT_DIR/enterprise"/*
rm -rf "$SCRIPT_DIR/messaging"/*

# Recreate directory structure
echo "📁 Recreating directory structure..."
mkdir -p "$SCRIPT_DIR/basic"
mkdir -p "$SCRIPT_DIR/standard"
mkdir -p "$SCRIPT_DIR/enterprise"
mkdir -p "$SCRIPT_DIR/messaging"

# Clean Docker test images
echo "🐳 Cleaning Docker test images..."
if command -v docker &> /dev/null; then
    # Remove test images
    docker images --format "table {{.Repository}}:{{.Tag}}" | grep "^test-" | awk '{print $1}' | xargs -r docker rmi || true
    
    # Clean up dangling images
    docker image prune -f || true
    
    echo "✅ Docker cleanup completed"
else
    echo "⚠️  Docker not found, skipping Docker cleanup"
fi

echo ""
echo "✅ Cleanup completed successfully!"
echo ""
echo "Test directories are now clean and ready for new tests." 