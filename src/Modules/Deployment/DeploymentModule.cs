using Microservice.Core.TemplateEngine.Abstractions;
using Microservice.Core.TemplateEngine.Configuration;

namespace Microservice.Modules.Deployment;

public class DeploymentModule : ITemplateModule
{
    public string Name => "Deployment";
    public string Description => "Generates Docker and Kubernetes deployment configurations with health checks and monitoring";

    public bool IsEnabled(TemplateConfiguration config)
    {
        var dockerEnabled = config.Features?.Deployment?.Docker?.ToLowerInvariant();
        var kubernetesEnabled = config.Features?.Deployment?.Kubernetes?.ToLowerInvariant();
        return dockerEnabled == "enabled" || dockerEnabled == "auto" || 
               kubernetesEnabled == "enabled" || kubernetesEnabled == "auto";
    }

    public async Task GenerateAsync(GenerationContext context)
    {
        var config = context.Configuration;
        var outputPath = Path.Combine(config.OutputPath, "deployment");

        // Create deployment structure
        await CreateDeploymentStructureAsync(outputPath, config);

        // Generate Docker files
        await GenerateDockerFilesAsync(outputPath, config);

        // Generate Kubernetes manifests if enabled
        if (ShouldGenerateKubernetes(config))
        {
            await GenerateKubernetesManifestsAsync(outputPath, config);
        }

        // Generate deployment scripts
        await GenerateDeploymentScriptsAsync(outputPath, config);

        // Generate project files (gitignore, dockerignore, Makefile)
        await GenerateProjectFilesAsync(config.OutputPath, config);
    }

    private async Task CreateDeploymentStructureAsync(string outputPath, TemplateConfiguration config)
    {
        Directory.CreateDirectory(outputPath);
        Directory.CreateDirectory(Path.Combine(outputPath, "docker"));
        Directory.CreateDirectory(Path.Combine(outputPath, "kubernetes"));
        Directory.CreateDirectory(Path.Combine(outputPath, "scripts"));
        Directory.CreateDirectory(Path.Combine(outputPath, "monitoring"));
    }

    private async Task GenerateDockerFilesAsync(string outputPath, TemplateConfiguration config)
    {
        // Generate main Dockerfile (Linux)
        var dockerfileContent = GenerateDockerfile(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, "docker", "Dockerfile"), dockerfileContent);

        // Generate Windows Dockerfile
        var dockerfileWindowsContent = GenerateDockerfileWindows(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, "docker", "Dockerfile.windows"), dockerfileWindowsContent);

        // Generate docker-compose.yml
        var dockerComposeContent = GenerateDockerCompose(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, "docker", "docker-compose.yml"), dockerComposeContent);

        // Generate docker-compose.override.yml for development
        var dockerComposeOverrideContent = GenerateDockerComposeOverride(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, "docker", "docker-compose.override.yml"), dockerComposeOverrideContent);

        // Generate docker-compose.windows.yml for Windows
        var dockerComposeWindowsContent = GenerateDockerComposeWindows(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, "docker", "docker-compose.windows.yml"), dockerComposeWindowsContent);

        // Generate .dockerignore
        var dockerIgnoreContent = GenerateDockerIgnore();
        await File.WriteAllTextAsync(Path.Combine(outputPath, "docker", ".dockerignore"), dockerIgnoreContent);

        // Generate health check script
        var healthCheckContent = GenerateHealthCheckScript(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, "docker", "healthcheck.sh"), healthCheckContent);

        // Generate Windows health check script
        var healthCheckWindowsContent = GenerateHealthCheckScriptWindows(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, "docker", "healthcheck.ps1"), healthCheckWindowsContent);
    }

    private async Task GenerateKubernetesManifestsAsync(string outputPath, TemplateConfiguration config)
    {
        var k8sPath = Path.Combine(outputPath, "kubernetes");

        // Generate namespace
        var namespaceContent = GenerateNamespace(config);
        await File.WriteAllTextAsync(Path.Combine(k8sPath, "namespace.yaml"), namespaceContent);

        // Generate deployment
        var deploymentContent = GenerateKubernetesDeployment(config);
        await File.WriteAllTextAsync(Path.Combine(k8sPath, "deployment.yaml"), deploymentContent);

        // Generate service
        var serviceContent = GenerateKubernetesService(config);
        await File.WriteAllTextAsync(Path.Combine(k8sPath, "service.yaml"), serviceContent);

        // Generate ingress
        var ingressContent = GenerateKubernetesIngress(config);
        await File.WriteAllTextAsync(Path.Combine(k8sPath, "ingress.yaml"), ingressContent);

        // Generate ConfigMap
        var configMapContent = GenerateConfigMap(config);
        await File.WriteAllTextAsync(Path.Combine(k8sPath, "configmap.yaml"), configMapContent);

        // Generate HPA (Horizontal Pod Autoscaler)
        var hpaContent = GenerateHPA(config);
        await File.WriteAllTextAsync(Path.Combine(k8sPath, "hpa.yaml"), hpaContent);

        // Generate PodDisruptionBudget
        var pdbContent = GeneratePodDisruptionBudget(config);
        await File.WriteAllTextAsync(Path.Combine(k8sPath, "pdb.yaml"), pdbContent);
    }

    private async Task GenerateDeploymentScriptsAsync(string outputPath, TemplateConfiguration config)
    {
        var scriptsPath = Path.Combine(outputPath, "scripts");

        // Generate build script
        var buildScriptContent = GenerateBuildScript(config);
        await File.WriteAllTextAsync(Path.Combine(scriptsPath, "build.sh"), buildScriptContent);

        // Generate deploy script
        var deployScriptContent = GenerateDeployScript(config);
        await File.WriteAllTextAsync(Path.Combine(scriptsPath, "deploy.sh"), deployScriptContent);

        // Generate monitoring setup
        var monitoringContent = GenerateMonitoringSetup(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, "monitoring", "prometheus.yml"), monitoringContent);
    }

    private bool ShouldGenerateKubernetes(TemplateConfiguration config)
    {
        var kubernetesEnabled = config.Features?.Deployment?.Kubernetes?.ToLowerInvariant();
        return kubernetesEnabled == "enabled" || kubernetesEnabled == "auto";
    }

    private string GenerateDockerfile(TemplateConfiguration config)
    {
        return $@"# Linux Dockerfile for production
# Build arguments
ARG VERSION=1.0.0

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0.16 AS build
WORKDIR /src

# Copy csproj files and restore dependencies (layer caching optimization)
COPY [""src/Api/{config.MicroserviceName}.Api/{config.MicroserviceName}.Api.csproj"", ""src/Api/{config.MicroserviceName}.Api/""]
COPY [""src/Application/{config.MicroserviceName}.Application/{config.MicroserviceName}.Application.csproj"", ""src/Application/{config.MicroserviceName}.Application/""]
COPY [""src/Infrastructure/{config.MicroserviceName}.Infrastructure/{config.MicroserviceName}.Infrastructure.csproj"", ""src/Infrastructure/{config.MicroserviceName}.Infrastructure/""]
COPY [""src/Domain/{config.MicroserviceName}.Domain/{config.MicroserviceName}.Domain.csproj"", ""src/Domain/{config.MicroserviceName}.Domain/""]

# Restore dependencies
RUN dotnet restore ""src/Api/{config.MicroserviceName}.Api/{config.MicroserviceName}.Api.csproj""

# Copy source code and build
COPY . .
WORKDIR ""/src/src/Api/{config.MicroserviceName}.Api""
RUN dotnet build ""{config.MicroserviceName}.Api.csproj"" -c Release -o /app/build --no-restore

# Publish stage
FROM build AS publish
RUN dotnet publish ""{config.MicroserviceName}.Api.csproj"" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0.16 AS final
WORKDIR /app

# Create non-root user for security
RUN groupadd --system --gid 1001 appgroup && \
    useradd --system --uid 1001 --gid appgroup --shell /bin/false appuser

# Install curl for health checks and clean up
RUN apt-get update && \
    apt-get install -y --no-install-recommends curl && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=publish /app/publish .

# Copy health check script
COPY deployment/docker/healthcheck.sh /app/healthcheck.sh
RUN chmod +x /app/healthcheck.sh

# Set ownership and switch to non-root user
RUN chown -R appuser:appgroup /app
USER appuser

# Configure environment
ARG VERSION=1.0.0
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    SERVICE_VERSION=$VERSION

# Add version label
LABEL version=$VERSION \
      service=""{config.MicroserviceName.ToLowerInvariant()}"" \
      maintainer=""Generated by MicroserviceKit""

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD /app/healthcheck.sh

ENTRYPOINT [""dotnet"", ""{config.MicroserviceName}.Api.dll""]";
    }

    private string GenerateDockerfileWindows(TemplateConfiguration config)
    {
        return $@"# Windows Dockerfile for local development
# Build arguments
ARG VERSION=1.0.0

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0.16-windowsservercore-ltsc2022 AS build
WORKDIR /src

# Copy csproj files and restore dependencies (layer caching optimization)
COPY [""src/Api/{config.MicroserviceName}.Api/{config.MicroserviceName}.Api.csproj"", ""src/Api/{config.MicroserviceName}.Api/""]
COPY [""src/Application/{config.MicroserviceName}.Application/{config.MicroserviceName}.Application.csproj"", ""src/Application/{config.MicroserviceName}.Application/""]
COPY [""src/Infrastructure/{config.MicroserviceName}.Infrastructure/{config.MicroserviceName}.Infrastructure.csproj"", ""src/Infrastructure/{config.MicroserviceName}.Infrastructure/""]
COPY [""src/Domain/{config.MicroserviceName}.Domain/{config.MicroserviceName}.Domain.csproj"", ""src/Domain/{config.MicroserviceName}.Domain/""]

# Restore dependencies
RUN dotnet restore ""src/Api/{config.MicroserviceName}.Api/{config.MicroserviceName}.Api.csproj""

# Copy source code and build
COPY . .
WORKDIR ""/src/src/Api/{config.MicroserviceName}.Api""
RUN dotnet build ""{config.MicroserviceName}.Api.csproj"" -c Release -o /app/build --no-restore

# Publish stage
FROM build AS publish
RUN dotnet publish ""{config.MicroserviceName}.Api.csproj"" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0.16-windowsservercore-ltsc2022 AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Copy health check script
COPY deployment/docker/healthcheck.ps1 /app/healthcheck.ps1

# Configure environment
ARG VERSION=1.0.0
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    ASPNETCORE_FORWARDEDHEADERS_ENABLED=true \
    SERVICE_VERSION=$VERSION

# Add version label
LABEL version=$VERSION \
      service=""{config.MicroserviceName.ToLowerInvariant()}"" \
      maintainer=""Generated by MicroserviceKit""

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD powershell -File /app/healthcheck.ps1

ENTRYPOINT [""dotnet"", ""{config.MicroserviceName}.Api.dll""]";
    }

    private string GenerateDockerCompose(TemplateConfiguration config)
    {
        var dbService = GenerateDockerComposeDatabase(config);
        var serviceName = config.MicroserviceName.ToLowerInvariant();
        
        return $@"version: '3.8'

services:
  {serviceName}-api:
    build:
      context: ..
      dockerfile: deployment/docker/Dockerfile
      args:
        - VERSION=${{VERSION:-1.0.0}}
    image: {serviceName}:${{VERSION:-1.0.0}}
    container_name: {serviceName}-api
    ports:
      - ""8080:8080""
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection={GetConnectionString(config)}
      - SERVICE_VERSION=${{VERSION:-1.0.0}}
    depends_on:
      - {GetDatabaseServiceName(config)}
    networks:
      - {serviceName}-network
    restart: unless-stopped
    healthcheck:
      test: [""CMD"", ""/app/healthcheck.sh""]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 60s

{dbService}

  # Redis for caching (optional)
  redis:
    image: redis:7-alpine
    container_name: {config.MicroserviceName.ToLowerInvariant()}-redis
    ports:
      - ""6379:6379""
    networks:
      - {config.MicroserviceName.ToLowerInvariant()}-network
    restart: unless-stopped
    command: redis-server --appendonly yes
    volumes:
      - redis-data:/data

  # Prometheus for monitoring
  prometheus:
    image: prom/prometheus:latest
    container_name: {config.MicroserviceName.ToLowerInvariant()}-prometheus
    ports:
      - ""9090:9090""
    volumes:
      - ./monitoring/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus-data:/prometheus
    networks:
      - {config.MicroserviceName.ToLowerInvariant()}-network
    restart: unless-stopped

networks:
  {config.MicroserviceName.ToLowerInvariant()}-network:
    driver: bridge

volumes:
  {GetDatabaseServiceName(config)}-data:
  redis-data:
  prometheus-data:";
    }

    private string GenerateDockerComposeDatabase(TemplateConfiguration config)
    {
        var provider = config.Features?.Persistence?.Provider?.ToLowerInvariant() ?? "postgresql";
        
        return provider switch
        {
            "postgresql" => $@"  postgres:
    image: postgres:15-alpine
    container_name: {config.MicroserviceName.ToLowerInvariant()}-postgres
    environment:
      - POSTGRES_DB={config.MicroserviceName.ToLowerInvariant()}db
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres123
    ports:
      - ""5432:5432""
    volumes:
      - postgres-data:/var/lib/postgresql/data
    networks:
      - {config.MicroserviceName.ToLowerInvariant()}-network
    restart: unless-stopped
    healthcheck:
      test: [""CMD-SHELL"", ""pg_isready -U postgres""]
      interval: 10s
      timeout: 5s
      retries: 5",

            "sqlserver" => $@"  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: {config.MicroserviceName.ToLowerInvariant()}-sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=SqlServer123!
      - MSSQL_PID=Express
    ports:
      - ""1433:1433""
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - {config.MicroserviceName.ToLowerInvariant()}-network
    restart: unless-stopped
    healthcheck:
      test: [""/opt/mssql-tools/bin/sqlcmd"", ""-S"", ""localhost"", ""-U"", ""sa"", ""-P"", ""SqlServer123!"", ""-Q"", ""SELECT 1""]
      interval: 10s
      timeout: 5s
      retries: 5",

            _ => $@"  # Using in-memory database - no external database service needed"
        };
    }

    private string GetDatabaseServiceName(TemplateConfiguration config)
    {
        var provider = config.Features?.Persistence?.Provider?.ToLowerInvariant() ?? "postgresql";
        return provider switch
        {
            "postgresql" => "postgres",
            "sqlserver" => "sqlserver",
            _ => "inmemory"
        };
    }

    private string GetConnectionString(TemplateConfiguration config)
    {
        var provider = config.Features?.Persistence?.Provider?.ToLowerInvariant() ?? "postgresql";
        return provider switch
        {
            "postgresql" => "Host=postgres;Database={config.MicroserviceName.ToLowerInvariant()}db;Username=postgres;Password=postgres123",
            "sqlserver" => "Server=sqlserver;Database={config.MicroserviceName}DB;User Id=sa;Password=SqlServer123!;TrustServerCertificate=true",
            _ => "Data Source=:memory:"
        };
    }

    private string GenerateDockerComposeOverride(TemplateConfiguration config)
    {
        return $@"version: '3.8'

services:
  {config.MicroserviceName.ToLowerInvariant()}-api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
    volumes:
      - ../src:/app/src:ro
    ports:
      - ""5000:8080""
    
  # Development tools
  seq:
    image: datalust/seq:latest
    container_name: {config.MicroserviceName.ToLowerInvariant()}-seq
    environment:
      - ACCEPT_EULA=Y
    ports:
      - ""5341:80""
    networks:
      - {config.MicroserviceName.ToLowerInvariant()}-network
    restart: unless-stopped

  # Grafana for monitoring dashboards
  grafana:
    image: grafana/grafana:latest
    container_name: {config.MicroserviceName.ToLowerInvariant()}-grafana
    ports:
      - ""3000:3000""
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin123
    volumes:
      - grafana-data:/var/lib/grafana
    networks:
      - {config.MicroserviceName.ToLowerInvariant()}-network
    restart: unless-stopped

volumes:
  grafana-data:";
    }

    private string GenerateDockerComposeWindows(TemplateConfiguration config)
    {
        var serviceName = config.MicroserviceName.ToLowerInvariant();
        
        return $@"# Windows Docker Compose for local development only
version: '3.8'

services:
  {serviceName}-api:
    build:
      context: ..
      dockerfile: deployment/docker/Dockerfile.windows
    container_name: {serviceName}-api-windows
    ports:
      - ""8080:8080""
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
    networks:
      - {serviceName}-network

networks:
  {serviceName}-network:
    driver: nat";
    }

    private string GenerateDockerIgnore()
    {
        return @"**/.dockerignore
**/.env
**/.git
**/.gitignore
**/.project
**/.settings
**/.toolstarget
**/.vs
**/.vscode
**/*.*proj.user
**/*.dbmdl
**/*.jfm
**/azds.yaml
**/bin
**/charts
**/docker-compose*
**/Dockerfile*
**/node_modules
**/npm-debug.log
**/obj
**/secrets.dev.yaml
**/values.dev.yaml
LICENSE
README.md
**/.idea
**/coverage
**/TestResults
**/*.trx
**/*.coverage
**/*.coveragexml
**/tests
**/deployment";
    }

    private string GenerateHealthCheckScript(TemplateConfiguration config)
    {
        return $@"#!/bin/bash
set -e

# Health check endpoint
HEALTH_URL=""http://localhost:8080/health""

# Perform health check
response=$(curl -s -o /dev/null -w ""%{{http_code}}"" $HEALTH_URL)

if [ $response -eq 200 ]; then
    echo ""Health check passed""
    exit 0
else
    echo ""Health check failed with status code: $response""
    exit 1
fi";
    }

    private string GenerateHealthCheckScriptWindows(TemplateConfiguration config)
    {
        return $@"# PowerShell Health Check Script for Windows containers
# Health check endpoint
$healthUrl = ""http://localhost:8080/health""

try {{
    # Perform health check
    $response = Invoke-WebRequest -Uri $healthUrl -UseBasicParsing -TimeoutSec 10
    
    if ($response.StatusCode -eq 200) {{
        Write-Host ""Health check passed""
        exit 0
    }} else {{
        Write-Host ""Health check failed with status code: $($response.StatusCode)""
        exit 1
    }}
}} catch {{
    Write-Host ""Health check failed with error: $($_.Exception.Message)""
    exit 1
}}";
    }

    private string GenerateKubernetesDeployment(TemplateConfiguration config)
    {
        return $@"apiVersion: apps/v1
kind: Deployment
metadata:
  name: {config.MicroserviceName.ToLowerInvariant()}-api
  namespace: {config.MicroserviceName.ToLowerInvariant()}
  labels:
    app: {config.MicroserviceName.ToLowerInvariant()}-api
    version: v1
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxUnavailable: 1
      maxSurge: 1
  selector:
    matchLabels:
      app: {config.MicroserviceName.ToLowerInvariant()}-api
  template:
    metadata:
      labels:
        app: {config.MicroserviceName.ToLowerInvariant()}-api
        version: v1
      annotations:
        prometheus.io/scrape: ""true""
        prometheus.io/port: ""8080""
        prometheus.io/path: ""/metrics""
    spec:
      serviceAccountName: {config.MicroserviceName.ToLowerInvariant()}-service-account
      securityContext:
        runAsNonRoot: true
        runAsUser: 1001
        fsGroup: 1001
      containers:
      - name: api
        image: {config.MicroserviceName.ToLowerInvariant()}-api:latest
        imagePullPolicy: Always
        ports:
        - containerPort: 8080
          name: http
          protocol: TCP
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: ""Production""
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: {config.MicroserviceName.ToLowerInvariant()}-secrets
              key: connection-string
        envFrom:
        - configMapRef:
            name: {config.MicroserviceName.ToLowerInvariant()}-config
        resources:
          requests:
            memory: ""256Mi""
            cpu: ""100m""
          limits:
            memory: ""512Mi""
            cpu: ""500m""
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 30
          timeoutSeconds: 10
          failureThreshold: 3
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 3
        startupProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 10
          timeoutSeconds: 5
          failureThreshold: 30
        securityContext:
          allowPrivilegeEscalation: false
          readOnlyRootFilesystem: true
          capabilities:
            drop:
            - ALL
        volumeMounts:
        - name: tmp
          mountPath: /tmp
        - name: app-logs
          mountPath: /app/logs
      volumes:
      - name: tmp
        emptyDir: {{}}
      - name: app-logs
        emptyDir: {{}}
      affinity:
        podAntiAffinity:
          preferredDuringSchedulingIgnoredDuringExecution:
          - weight: 100
            podAffinityTerm:
              labelSelector:
                matchExpressions:
                - key: app
                  operator: In
                  values:
                  - {config.MicroserviceName.ToLowerInvariant()}-api
              topologyKey: kubernetes.io/hostname";
    }

    private string GenerateNamespace(TemplateConfiguration config)
    {
        return $@"apiVersion: v1
kind: Namespace
metadata:
  name: {config.MicroserviceName.ToLowerInvariant()}
  labels:
    name: {config.MicroserviceName.ToLowerInvariant()}
    istio-injection: enabled
---
apiVersion: v1
kind: ServiceAccount
metadata:
  name: {config.MicroserviceName.ToLowerInvariant()}-service-account
  namespace: {config.MicroserviceName.ToLowerInvariant()}
automountServiceAccountToken: false";
    }

    private string GenerateKubernetesService(TemplateConfiguration config)
    {
        return $@"apiVersion: v1
kind: Service
metadata:
  name: {config.MicroserviceName.ToLowerInvariant()}-api-service
  namespace: {config.MicroserviceName.ToLowerInvariant()}
  labels:
    app: {config.MicroserviceName.ToLowerInvariant()}-api
spec:
  type: ClusterIP
  ports:
  - port: 80
    targetPort: 8080
    protocol: TCP
    name: http
  selector:
    app: {config.MicroserviceName.ToLowerInvariant()}-api
---
apiVersion: v1
kind: Service
metadata:
  name: {config.MicroserviceName.ToLowerInvariant()}-api-headless
  namespace: {config.MicroserviceName.ToLowerInvariant()}
  labels:
    app: {config.MicroserviceName.ToLowerInvariant()}-api
spec:
  type: ClusterIP
  clusterIP: None
  ports:
  - port: 80
    targetPort: 8080
    protocol: TCP
    name: http
  selector:
    app: {config.MicroserviceName.ToLowerInvariant()}-api";
    }

    private string GenerateKubernetesIngress(TemplateConfiguration config)
    {
        return $@"apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: {config.MicroserviceName.ToLowerInvariant()}-api-ingress
  namespace: {config.MicroserviceName.ToLowerInvariant()}
  annotations:
    kubernetes.io/ingress.class: nginx
    nginx.ingress.kubernetes.io/rewrite-target: /
    nginx.ingress.kubernetes.io/ssl-redirect: ""true""
    nginx.ingress.kubernetes.io/rate-limit: ""100""
    nginx.ingress.kubernetes.io/rate-limit-window: ""1m""
    cert-manager.io/cluster-issuer: letsencrypt-prod
spec:
  tls:
  - hosts:
    - {config.MicroserviceName.ToLowerInvariant()}.example.com
    secretName: {config.MicroserviceName.ToLowerInvariant()}-tls
  rules:
  - host: {config.MicroserviceName.ToLowerInvariant()}.example.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: {config.MicroserviceName.ToLowerInvariant()}-api-service
            port:
              number: 80";
    }

    private string GenerateConfigMap(TemplateConfiguration config)
    {
        return $@"apiVersion: v1
kind: ConfigMap
metadata:
  name: {config.MicroserviceName.ToLowerInvariant()}-config
  namespace: {config.MicroserviceName.ToLowerInvariant()}
data:
  ASPNETCORE_ENVIRONMENT: ""Production""
  Logging__LogLevel__Default: ""Information""
  Logging__LogLevel__Microsoft: ""Warning""
  Features__RateLimiting__Enabled: ""true""
  Features__RateLimiting__RequestsPerMinute: ""100""
  Features__Caching__Enabled: ""true""
  Features__Caching__DefaultTtlMinutes: ""30""
---
apiVersion: v1
kind: Secret
metadata:
  name: {config.MicroserviceName.ToLowerInvariant()}-secrets
  namespace: {config.MicroserviceName.ToLowerInvariant()}
type: Opaque
stringData:
  connection-string: ""{GetConnectionString(config)}""
  jwt-secret: ""your-super-secret-jwt-key-here""
  api-key: ""your-api-key-here""";
    }

    private string GenerateHPA(TemplateConfiguration config)
    {
        return $@"apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: {config.MicroserviceName.ToLowerInvariant()}-api-hpa
  namespace: {config.MicroserviceName.ToLowerInvariant()}
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: {config.MicroserviceName.ToLowerInvariant()}-api
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
  behavior:
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
      - type: Percent
        value: 10
        periodSeconds: 60
    scaleUp:
      stabilizationWindowSeconds: 60
      policies:
      - type: Percent
        value: 50
        periodSeconds: 60";
    }

    private string GeneratePodDisruptionBudget(TemplateConfiguration config)
    {
        return $@"apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: {config.MicroserviceName.ToLowerInvariant()}-api-pdb
  namespace: {config.MicroserviceName.ToLowerInvariant()}
spec:
  minAvailable: 1
  selector:
    matchLabels:
      app: {config.MicroserviceName.ToLowerInvariant()}-api";
    }

    private string GenerateBuildScript(TemplateConfiguration config)
    {
        return $@"#!/bin/bash
set -e

echo ""Building {config.MicroserviceName} microservice...""

# Build Docker image
docker build -t {config.MicroserviceName.ToLowerInvariant()}-api:latest -f deployment/docker/Dockerfile .

# Tag for registry (update with your registry)
docker tag {config.MicroserviceName.ToLowerInvariant()}-api:latest your-registry.com/{config.MicroserviceName.ToLowerInvariant()}-api:latest

echo ""Build completed successfully!""
echo ""To push to registry, run: docker push your-registry.com/{config.MicroserviceName.ToLowerInvariant()}-api:latest""";
    }

    private string GenerateDeployScript(TemplateConfiguration config)
    {
        return $@"#!/bin/bash
set -e

NAMESPACE=""{config.MicroserviceName.ToLowerInvariant()}""
DEPLOYMENT_PATH=""deployment/kubernetes""

echo ""Deploying {config.MicroserviceName} to Kubernetes...""

# Create namespace if it doesn't exist
kubectl apply -f $DEPLOYMENT_PATH/namespace.yaml

# Apply ConfigMap and Secrets
kubectl apply -f $DEPLOYMENT_PATH/configmap.yaml

# Apply Deployment
kubectl apply -f $DEPLOYMENT_PATH/deployment.yaml

# Apply Service
kubectl apply -f $DEPLOYMENT_PATH/service.yaml

# Apply Ingress
kubectl apply -f $DEPLOYMENT_PATH/ingress.yaml

# Apply HPA
kubectl apply -f $DEPLOYMENT_PATH/hpa.yaml

# Apply PDB
kubectl apply -f $DEPLOYMENT_PATH/pdb.yaml

echo ""Waiting for deployment to be ready...""
kubectl wait --for=condition=available --timeout=300s deployment/{config.MicroserviceName.ToLowerInvariant()}-api -n $NAMESPACE

echo ""Deployment completed successfully!""
echo ""Service URL: http://{config.MicroserviceName.ToLowerInvariant()}.example.com""";
    }

    private string GenerateMonitoringSetup(TemplateConfiguration config)
    {
        return $@"global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  # - ""first_rules.yml""
  # - ""second_rules.yml""

scrape_configs:
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']

  - job_name: '{config.MicroserviceName.ToLowerInvariant()}-api'
    static_configs:
      - targets: ['{config.MicroserviceName.ToLowerInvariant()}-api:8080']
    metrics_path: '/metrics'
    scrape_interval: 30s

  - job_name: 'kubernetes-pods'
    kubernetes_sd_configs:
      - role: pod
        namespaces:
          names:
            - {config.MicroserviceName.ToLowerInvariant()}
    relabel_configs:
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_scrape]
        action: keep
        regex: true
      - source_labels: [__meta_kubernetes_pod_annotation_prometheus_io_path]
        action: replace
        target_label: __metrics_path__
        regex: (.+)
      - source_labels: [__address__, __meta_kubernetes_pod_annotation_prometheus_io_port]
        action: replace
        regex: ([^:]+)(?::\d+)?;(\d+)
        replacement: $1:$2
        target_label: __address__";
    }

    private async Task GenerateProjectFilesAsync(string outputPath, TemplateConfiguration config)
    {
        // Generate .gitignore
        var gitIgnoreContent = GenerateGitIgnore(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, ".gitignore"), gitIgnoreContent);

        // Generate .dockerignore (in root for better Docker context)
        var dockerIgnoreContent = GenerateProjectDockerIgnore(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, ".dockerignore"), dockerIgnoreContent);

        // Generate Makefile
        var makefileContent = GenerateMakefile(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, "Makefile"), makefileContent);

        // Generate VERSION file
        var versionContent = GenerateVersionFile(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, "VERSION"), versionContent);

        // Generate .env.example
        var envExampleContent = GenerateEnvExample(config);
        await File.WriteAllTextAsync(Path.Combine(outputPath, ".env.example"), envExampleContent);
    }

    private string GenerateGitIgnore(TemplateConfiguration config)
    {
        return @"# .NET build artifacts
**/bin/
**/obj/
**/out/

# Visual Studio / VS Code
.vs/
.vscode/
*.user
*.suo
*.cache
.DS_Store

# NuGet packages
packages/
*.nupkg

# Test results
TestResults/
*.trx
*.coverage
*.coveragexml
coverage/

# Logs
logs/
*.log

# Runtime files
*.pid
*.seed
*.pid.lock

# Environment files
.env
.env.local
.env.development
.env.test
.env.production

# Docker
.dockerignore

# IDE
.idea/
*.swp
*.swo
*~

# OS generated files
.DS_Store
.DS_Store?
._*
.Spotlight-V100
.Trashes
ehthumbs.db
Thumbs.db

# Temporary files
*.tmp
*.temp

# Build outputs
dist/
build/

# Dependency directories
node_modules/

# Migration history (keep versioned)
# .microservice-history.json

# Local development
local/
temp/
scratch/";
    }

    private string GenerateProjectDockerIgnore(TemplateConfiguration config)
    {
        return @"# Git
.git
.gitignore
.gitattributes

# CI/CD
.github/
.gitlab-ci.yml
azure-pipelines.yml
Jenkinsfile

# Documentation
README.md
CHANGELOG.md
LICENSE
docs/
*.md

# IDE
.vs/
.vscode/
.idea/
*.swp
*.swo

# Build artifacts
**/bin/
**/obj/
**/out/
**/TestResults/
**/.nuget/

# Test files
**/*Tests/
**/tests/
*.trx
*.coverage

# Environment files
.env*
!.env.example

# Logs
logs/
*.log

# Temporary files
*.tmp
*.temp
*~

# OS files
.DS_Store
Thumbs.db

# Docker
Dockerfile*
docker-compose*
.dockerignore

# Deployment
deployment/
k8s/
kubernetes/
helm/

# Examples
examples/
samples/";
    }

    private string GenerateMakefile(TemplateConfiguration config)
    {
        var serviceName = config.MicroserviceName.ToLowerInvariant();
        
        return $@"# {config.MicroserviceName} Makefile
# Version management and common tasks

# Variables
SERVICE_NAME := {serviceName}
VERSION := $(shell cat VERSION 2>/dev/null || echo ""1.0.0"")
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
	@echo ""$(BLUE){config.MicroserviceName} - Available commands:$(NC)""
	@awk 'BEGIN {{FS = "":.*?## ""}} /^[a-zA-Z_-]+:.*?## / {{printf ""  $(GREEN)%-15s$(NC) %s\n"", $$1, $$2}}' $(MAKEFILE_LIST)

# Version management
version: ## Show current version
	@echo ""Current version: $(YELLOW)$(VERSION)$(NC)""

version-patch: ## Bump patch version (1.0.0 -> 1.0.1)
	@echo ""$(YELLOW)Bumping patch version...$(NC)""
	@echo $(VERSION) | awk -F. '{{$$3++; print $$1"".""$$2"".""$$3}}' > VERSION
	@echo ""New version: $(GREEN)$$(cat VERSION)$(NC)""

version-minor: ## Bump minor version (1.0.0 -> 1.1.0)
	@echo ""$(YELLOW)Bumping minor version...$(NC)""
	@echo $(VERSION) | awk -F. '{{$$2++; $$3=0; print $$1"".""$$2"".""$$3}}' > VERSION
	@echo ""New version: $(GREEN)$$(cat VERSION)$(NC)""

version-major: ## Bump major version (1.0.0 -> 2.0.0)
	@echo ""$(YELLOW)Bumping major version...$(NC)""
	@echo $(VERSION) | awk -F. '{{$$1++; $$2=0; $$3=0; print $$1"".""$$2"".""$$3}}' > VERSION
	@echo ""New version: $(GREEN)$$(cat VERSION)$(NC)""

# Build and test
restore: ## Restore NuGet packages
	@echo ""$(BLUE)Restoring packages...$(NC)""
	dotnet restore

build: restore ## Build the solution
	@echo ""$(BLUE)Building solution...$(NC)""
	dotnet build --configuration Release --no-restore

test: build ## Run all tests
	@echo ""$(BLUE)Running tests...$(NC)""
	dotnet test --configuration Release --no-build --verbosity normal

test-coverage: ## Run tests with coverage
	@echo ""$(BLUE)Running tests with coverage...$(NC)""
	dotnet test --configuration Release --collect:""XPlat Code Coverage"" --results-directory ./TestResults

clean: ## Clean build artifacts
	@echo ""$(BLUE)Cleaning...$(NC)""
	dotnet clean
	rm -rf **/bin **/obj TestResults/

# Docker commands
docker-build: ## Build Docker image (Linux)
	@echo ""$(BLUE)Building Docker image...$(NC)""
	docker build -f deployment/docker/Dockerfile -t $(DOCKER_IMAGE):$(DOCKER_TAG) -t $(DOCKER_IMAGE):latest --build-arg VERSION=$(VERSION) .

docker-build-windows: ## Build Docker image (Windows)
	@echo ""$(BLUE)Building Windows Docker image...$(NC)""
	docker build -f deployment/docker/Dockerfile.windows -t $(DOCKER_IMAGE):$(DOCKER_TAG)-windows --build-arg VERSION=$(VERSION) .

docker-push: docker-build ## Push Docker image to registry
	@echo ""$(BLUE)Pushing Docker image...$(NC)""
	docker push $(DOCKER_IMAGE):$(DOCKER_TAG)
	docker push $(DOCKER_IMAGE):latest

docker-run: ## Run Docker container locally (Linux)
	@echo ""$(BLUE)Running Docker container...$(NC)""
	docker run -d -p 8080:8080 --name $(SERVICE_NAME) $(DOCKER_IMAGE):latest

docker-run-windows: ## Run Docker container locally (Windows)
	@echo ""$(BLUE)Running Windows Docker container...$(NC)""
	docker run -d -p 8080:8080 --name $(SERVICE_NAME)-windows $(DOCKER_IMAGE):$(DOCKER_TAG)-windows

docker-stop: ## Stop Docker container
	@echo ""$(BLUE)Stopping Docker container...$(NC)""
	docker stop $(SERVICE_NAME) || true
	docker rm $(SERVICE_NAME) || true
	docker stop $(SERVICE_NAME)-windows || true
	docker rm $(SERVICE_NAME)-windows || true

docker-compose-up: ## Start with docker-compose
	@echo ""$(BLUE)Starting with docker-compose...$(NC)""
	cd deployment/docker && docker-compose up -d

docker-compose-down: ## Stop docker-compose
	@echo ""$(BLUE)Stopping docker-compose...$(NC)""
	cd deployment/docker && docker-compose down

# Kubernetes commands
k8s-deploy: ## Deploy to Kubernetes
	@echo ""$(BLUE)Deploying to Kubernetes...$(NC)""
	kubectl apply -f deployment/kubernetes/

k8s-delete: ## Delete from Kubernetes
	@echo ""$(BLUE)Deleting from Kubernetes...$(NC)""
	kubectl delete -f deployment/kubernetes/

k8s-status: ## Show Kubernetes status
	@echo ""$(BLUE)Kubernetes status:$(NC)""
	kubectl get pods,services,ingress -n $(NAMESPACE)

k8s-logs: ## Show Kubernetes logs
	@echo ""$(BLUE)Kubernetes logs:$(NC)""
	kubectl logs -f deployment/$(SERVICE_NAME)-api -n $(NAMESPACE)

# Development commands
dev: ## Start development environment (Linux)
	@echo ""$(BLUE)Starting development environment...$(NC)""
	cd deployment/docker && docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

dev-windows: ## Start development environment (Windows)
	@echo ""$(BLUE)Starting Windows development environment...$(NC)""
	cd deployment/docker && docker-compose -f docker-compose.windows.yml up -d

dev-logs: ## Show development logs
	@echo ""$(BLUE)Development logs:$(NC)""
	cd deployment/docker && docker-compose logs -f

dev-stop: ## Stop development environment
	@echo ""$(BLUE)Stopping development environment...$(NC)""
	cd deployment/docker && docker-compose down

dev-stop-windows: ## Stop Windows development environment
	@echo ""$(BLUE)Stopping Windows development environment...$(NC)""
	cd deployment/docker && docker-compose -f docker-compose.windows.yml down

# Database commands
db-migrate: ## Run database migrations
	@echo ""$(BLUE)Running database migrations...$(NC)""
	dotnet ef database update --project src/Infrastructure/{config.MicroserviceName}.Infrastructure

db-migration: ## Create new migration (usage: make db-migration NAME=MigrationName)
	@echo ""$(BLUE)Creating migration: $(NAME)$(NC)""
	dotnet ef migrations add $(NAME) --project src/Infrastructure/{config.MicroserviceName}.Infrastructure

# Release commands
release-patch: version-patch docker-build docker-push ## Release patch version
	@echo ""$(GREEN)Released patch version: $$(cat VERSION)$(NC)""

release-minor: version-minor docker-build docker-push ## Release minor version
	@echo ""$(GREEN)Released minor version: $$(cat VERSION)$(NC)""

release-major: version-major docker-build docker-push ## Release major version
	@echo ""$(GREEN)Released major version: $$(cat VERSION)$(NC)""

# Git commands
git-tag: ## Create git tag with current version
	@echo ""$(BLUE)Creating git tag: v$(VERSION)$(NC)""
	git tag -a v$(VERSION) -m ""Release version $(VERSION)""
	git push origin v$(VERSION)

# Health check
health: ## Check service health
	@echo ""$(BLUE)Checking service health...$(NC)""
	curl -f http://localhost:8080/health || echo ""$(RED)Service is not healthy$(NC)""

# Monitoring
logs: ## Show application logs
	@echo ""$(BLUE)Application logs:$(NC)""
	docker logs -f $(SERVICE_NAME) 2>/dev/null || echo ""$(YELLOW)Container not running$(NC)""

metrics: ## Show metrics endpoint
	@echo ""$(BLUE)Metrics endpoint:$(NC)""
	curl -s http://localhost:8080/metrics || echo ""$(RED)Metrics not available$(NC)""";
    }

    private string GenerateVersionFile(TemplateConfiguration config)
    {
        return "1.0.0";
    }

    private string GenerateEnvExample(TemplateConfiguration config)
    {
        var provider = config.Features?.Persistence?.Provider?.ToLowerInvariant() ?? "postgresql";
        
        var connectionString = provider switch
        {
            "postgresql" => "Host=localhost;Database={config.MicroserviceName.ToLowerInvariant()}db;Username=postgres;Password=your_password_here",
            "sqlserver" => "Server=localhost;Database={config.MicroserviceName}DB;User Id=sa;Password=your_password_here;TrustServerCertificate=true",
            _ => "Data Source=:memory:"
        };

        return $@"# Environment Configuration Example
# Copy this file to .env and update with your values

# Application
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080

# Database
ConnectionStrings__DefaultConnection={connectionString}

# Logging
Logging__LogLevel__Default=Information
Logging__LogLevel__Microsoft.AspNetCore=Warning

# JWT (if using authentication)
JWT__SecretKey=your-super-secret-key-here-minimum-32-characters
JWT__Issuer={config.MicroserviceName}
JWT__Audience={config.MicroserviceName}
JWT__ExpirationMinutes=60

# Redis (if using caching)
Redis__ConnectionString=localhost:6379

# Monitoring
PROMETHEUS_ENABLED=true
HEALTH_CHECKS_ENABLED=true

# Rate Limiting
RateLimit__PermitLimit=100
RateLimit__Window=00:01:00

# CORS
CORS__AllowedOrigins=http://localhost:3000,http://localhost:4200

# External Services
# ExternalService__BaseUrl=https://api.example.com
# ExternalService__ApiKey=your-api-key-here

# Docker specific
DOTNET_RUNNING_IN_CONTAINER=true
DOTNET_USE_POLLING_FILE_WATCHER=true";
    }
} 