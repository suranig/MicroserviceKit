# PowerShell Health Check Script for Windows containers
# Health check endpoint
$healthUrl = "http://localhost:8080/health"

try {
    # Perform health check
    $response = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec 10
    
    if ($response.StatusCode -eq 200) {
        Write-Host "Health check passed"
        exit 0
    } else {
        Write-Host "Health check failed with status code: $($response.StatusCode)"
        exit 1
    }
} catch {
    Write-Host "Health check failed with error: $($_.Exception.Message)"
    exit 1
}