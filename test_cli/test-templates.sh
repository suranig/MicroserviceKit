#!/bin/bash

# Test all templates script
# Tests all templates in the templates/ directory

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "🧪 Testing all templates..."
echo "=========================="

# Test templates
TEMPLATES=(
    "templates/levels/minimal-service.json"
    "templates/levels/standard-service.json" 
    "templates/levels/enterprise-service.json"
    "templates/examples/complete-microservice.json"
)

FAILED_TESTS=0
TOTAL_TESTS=${#TEMPLATES[@]}

for template in "${TEMPLATES[@]}"; do
    template_name=$(basename "$template" .json)
    test_dir="$SCRIPT_DIR/template-tests/$template_name"
    
    echo ""
    echo "🔧 Testing template: $template"
    echo "   Output directory: $test_dir"
    
    # Create test directory
    mkdir -p "$test_dir"
    cd "$test_dir"
    rm -rf *
    
    # Generate microservice
    if dotnet run --project "$PROJECT_ROOT/src/CLI/MicroserviceGenerator.CLI/MicroserviceGenerator.CLI.csproj" -- new "Test${template_name^}" --config "$PROJECT_ROOT/$template" --output .; then
        echo "   ✅ Generation successful"
        
        # Try to build
        if dotnet build > /dev/null 2>&1; then
            echo "   ✅ Build successful"
        else
            echo "   ❌ Build failed"
            FAILED_TESTS=$((FAILED_TESTS + 1))
        fi
    else
        echo "   ❌ Generation failed"
        FAILED_TESTS=$((FAILED_TESTS + 1))
    fi
done

echo ""
echo "📊 Test Results"
echo "==============="
echo "Total templates tested: $TOTAL_TESTS"
echo "Successful: $((TOTAL_TESTS - FAILED_TESTS))"
echo "Failed: $FAILED_TESTS"

if [ $FAILED_TESTS -eq 0 ]; then
    echo ""
    echo "🎉 All templates working correctly!"
    exit 0
else
    echo ""
    echo "❌ Some templates failed. Check the output above."
    exit 1
fi 