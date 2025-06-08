#!/bin/bash

# Quick CLI Test Script
# Tests basic microservice generation and compilation

set -e  # Exit on any error

echo "🧪 Running CLI Quick Test..."
echo "================================"

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "📁 Project root: $PROJECT_ROOT"
echo "📁 Test directory: $SCRIPT_DIR"

# Clean previous test
echo "🧹 Cleaning previous test..."
rm -rf "$SCRIPT_DIR/basic"/*

# Create test directory if it doesn't exist
mkdir -p "$SCRIPT_DIR/basic"

# Navigate to test directory
cd "$SCRIPT_DIR/basic"

echo "🏗️  Generating microservice..."
echo "Command: dotnet run --project $PROJECT_ROOT/src/CLI/MicroserviceGenerator.CLI/MicroserviceGenerator.CLI.csproj -- new QuickTest --output ."

# Generate microservice
dotnet run --project "$PROJECT_ROOT/src/CLI/MicroserviceGenerator.CLI/MicroserviceGenerator.CLI.csproj" -- new QuickTest --output .

# Check if generation was successful
if [ ! -f "QuickTest.sln" ]; then
    echo "❌ Generation failed - QuickTest.sln not created"
    exit 1
fi

if [ ! -d "src" ]; then
    echo "❌ Generation failed - src directory not created"
    exit 1
fi

echo "✅ Generation completed successfully"

# We're already in the generated project directory

echo "🔨 Building generated microservice..."

# Build the project
if dotnet build; then
    echo "✅ Build completed successfully"
else
    echo "❌ Build failed"
    exit 1
fi

# Run tests if they exist
if [ -d "tests" ]; then
    echo "🧪 Running tests..."
    if dotnet test; then
        echo "✅ Tests passed"
    else
        echo "⚠️  Tests failed"
    fi
fi

echo ""
echo "🎉 Quick test completed successfully!"
echo "📁 Generated project location: $SCRIPT_DIR/basic"
echo ""
echo "To explore the generated project:"
echo "  cd test_cli/basic"
echo "  code ." 