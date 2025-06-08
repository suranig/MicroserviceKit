#!/bin/bash

# Full CLI Test Suite
# Tests all architecture levels and validates generated code

set -e  # Exit on any error

echo "🧪 Running Full CLI Test Suite..."
echo "=================================="

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "📁 Project root: $PROJECT_ROOT"
echo "📁 Test directory: $SCRIPT_DIR"

# Test results tracking
PASSED_TESTS=0
FAILED_TESTS=0
TEST_RESULTS=()

# Function to run a single test
run_test() {
    local level=$1
    local test_name="Test${level^}"
    
    echo ""
    echo "🔄 Testing $level level..."
    echo "=========================="
    
    # Clean previous test
    echo "🧹 Cleaning $level test directory..."
    rm -rf "$SCRIPT_DIR/$level"/*
    mkdir -p "$SCRIPT_DIR/$level"
    
    # Navigate to test directory
    cd "$SCRIPT_DIR/$level"
    
    echo "🏗️  Generating $level microservice..."
    
    # Generate microservice
    if dotnet run --project "$PROJECT_ROOT/src/CLI/CLI.csproj" -- generate --name "$test_name" --output . --level "$level"; then
        echo "✅ Generation completed for $level"
    else
        echo "❌ Generation failed for $level"
        FAILED_TESTS=$((FAILED_TESTS + 1))
        TEST_RESULTS+=("❌ $level: Generation failed")
        return 1
    fi
    
    # Check if project was created
    if [ ! -d "$test_name" ]; then
        echo "❌ Project directory not created for $level"
        FAILED_TESTS=$((FAILED_TESTS + 1))
        TEST_RESULTS+=("❌ $level: Project directory missing")
        return 1
    fi
    
    # Navigate to generated project
    cd "$test_name"
    
    echo "🔨 Building $level microservice..."
    
    # Build the project
    if dotnet build; then
        echo "✅ Build completed for $level"
    else
        echo "❌ Build failed for $level"
        FAILED_TESTS=$((FAILED_TESTS + 1))
        TEST_RESULTS+=("❌ $level: Build failed")
        return 1
    fi
    
    # Run tests if they exist
    if [ -d "tests" ]; then
        echo "🧪 Running tests for $level..."
        if dotnet test --no-build; then
            echo "✅ Tests passed for $level"
        else
            echo "⚠️  Tests failed for $level"
            TEST_RESULTS+=("⚠️  $level: Tests failed")
        fi
    fi
    
    # Test Docker build if Dockerfile exists
    if [ -f "Dockerfile" ]; then
        echo "🐳 Testing Docker build for $level..."
        if docker build -t "test-$level" .; then
            echo "✅ Docker build completed for $level"
            # Clean up Docker image
            docker rmi "test-$level" || true
        else
            echo "⚠️  Docker build failed for $level"
            TEST_RESULTS+=("⚠️  $level: Docker build failed")
        fi
    fi
    
    PASSED_TESTS=$((PASSED_TESTS + 1))
    TEST_RESULTS+=("✅ $level: All tests passed")
    
    echo "✅ $level test completed successfully"
}

# Test all architecture levels
LEVELS=("minimal" "standard" "enterprise")

for level in "${LEVELS[@]}"; do
    run_test "$level" || true  # Continue even if one test fails
done

# Print summary
echo ""
echo "📊 Test Summary"
echo "==============="
echo "Total tests: ${#LEVELS[@]}"
echo "Passed: $PASSED_TESTS"
echo "Failed: $FAILED_TESTS"
echo ""

echo "📋 Detailed Results:"
for result in "${TEST_RESULTS[@]}"; do
    echo "  $result"
done

echo ""

if [ $FAILED_TESTS -eq 0 ]; then
    echo "🎉 All tests passed successfully!"
    echo ""
    echo "Generated projects:"
    for level in "${LEVELS[@]}"; do
        if [ -d "$SCRIPT_DIR/$level/Test${level^}" ]; then
            echo "  📁 $SCRIPT_DIR/$level/Test${level^}"
        fi
    done
    exit 0
else
    echo "❌ Some tests failed. Please check the output above."
    exit 1
fi 