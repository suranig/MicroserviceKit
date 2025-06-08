# 🚀 Microservice Generator - Plan Rozwoju

## 📊 **OBECNY STAN - CO JEST ZROBIONE**

### ✅ **Podstawowa Architektura**
- [x] Domain Layer z AggregateKit integration
- [x] Application Layer z Wolverine (CQRS) - podstawowa implementacja
- [x] Infrastructure Layer z Repository pattern
- [x] API Layer z Minimal API - podstawowa implementacja
- [x] Testy jednostkowe - podstawowe

### ✅ **Template Engine Core**
- [x] Konfiguracja JSON z różnymi poziomami
- [x] DDDModule do generowania agregatów i eventów
- [x] ApplicationModule do generowania CQRS handlers (KOMPLETNY)
- [x] CLI z komendami new, migrate, history
- [x] System migracji między poziomami architektury
- [x] Historia migracji w JSON

### ✅ **Parametryzacja**
- [x] Dynamiczne nazwy serwisów i namespace
- [x] Tryb interaktywny
- [x] Różne poziomy złożoności (minimal/standard/enterprise)
- [x] Inteligentne reguły architektury

---

## 🎯 **PLAN ROZSZERZENIA - DO ZROBIENIA**

### 🔧 **1. API Layer Generator** 
**Priorytet: WYSOKI**

#### 1.1 REST API Module
- [x] **REST Controllers Generator z ASP.NesT**
  - [x] CRUD endpoints (GET,COUNT, POST, PUT, DELETE)
  - [x] Paging, filtering, sorting
  - [x] Response DTOs mapping
  - [x] HTTP status codes handling
  - [x] OpenAPI/Swagger documentation

- [ ] **Minimal API Generator** 
  - [ ] Endpoint mapping z IMessageBus
  - [ ] Route groups per aggregate
  - [ ] Request/Response validation
  - [ ] Error handling middleware

- [ ] **API Configuration**
  - [ ] Authentication (JWT, OAuth)
  - [ ] CORS configuration
  - [ ] Rate limiting
  - [ ] API versioning

#### 1.2 gRPC API Module
- [ ] **gRPC Service Generator**
  - [ ] .proto files generation
  - [ ] Service implementation
  - [ ] Message mapping
  - [ ] Error handling

- [ ] **gRPC Configuration**
  - [ ] Server configuration
  - [ ] Client generation
  - [ ] Health checks

### 🧪 **2. Test Generator Module**
**Priorytet: WYSOKI** ⭐

#### 2.1 Unit Tests
- [x] **Domain Tests** ✅ KOMPLETNY
  - [x] Aggregate behavior tests
  - [x] Domain event tests
  - [x] Value object tests
  - [x] Business rule validation tests

- [x] **Application Tests** ✅ KOMPLETNY
  - [x] Command handler tests
  - [x] Query handler tests
  - [x] Validation tests
  - [x] Behavior tests (logging, validation)

- [x] **Infrastructure Tests** ✅ KOMPLETNY
  - [x] Repository tests
  - [x] Database integration tests
  - [x] External service mocks

#### 2.2 Integration Tests
- [ ] **API Tests**
  - [ ] REST endpoint tests
  - [ ] gRPC service tests
  - [ ] Authentication tests
  - [ ] Error scenario tests

- [ ] **End-to-End Tests**
  - [ ] Full workflow tests
  - [ ] Database persistence tests
  - [ ] Message publishing tests

#### 2.3 Test Infrastructure
- [ ] **Test Utilities**
  - [ ] Test data builders
  - [ ] Mock factories
  - [ ] Test database setup
  - [ ] Test containers (Docker)

### 🗄️ **3. Infrastructure Layer Generator**
**Priorytet: ŚREDNI**

#### 3.1 Persistence Module
- [ ] **Repository Implementation**
  - [ ] Entity Framework Core repositories
  - [ ] MongoDB repositories
  - [ ] In-memory repositories
  - [ ] Unit of Work pattern

- [ ] **Database Configuration**
  - [ ] DbContext generation
  - [ ] Entity configurations
  - [ ] Migrations
  - [ ] Connection string management

#### 3.2 External Services Module
- [ ] **HTTP Clients**
  - [ ] Typed HTTP clients
  - [ ] Polly resilience patterns
  - [ ] Authentication handling
  - [ ] Response mapping

- [ ] **Message Brokers**
  - [ ] RabbitMQ integration
  - [ ] Azure Service Bus
  - [ ] Event publishing
  - [ ] Message consumers

### 📦 **4. Deployment & DevOps Module**
**Priorytet: ŚREDNI**

#### 4.1 Containerization
- [ ] **Docker Infrastructure**
  - [ ] **microservice.Dockerfile** - główny mikrousługa
  - [ ] **rabbitmq.Dockerfile** - message broker (jeśli eventy)
  - [ ] **mongodb.Dockerfile** - read model (jeśli CQRS)
  - [ ] **postgres.Dockerfile** - write model (jeśli PostgreSQL)
  - [ ] **mysql.Dockerfile** - write model (jeśli MySQL)
  - [ ] **docker-compose.yml** - orkiestracja wszystkich kontenerów
  - [ ] Multi-stage builds dla mikrousługi
  - [ ] Health checks dla wszystkich serwisów

- [ ] **Kubernetes**
  - [ ] **Deployment** manifests dla mikrousługi
  - [ ] **HPA** (Horizontal Pod Autoscaler)
  - [ ] **Services** (ClusterIP, LoadBalancer)
  - [ ] **Pods** configuration
  - [ ] ConfigMaps i Secrets
  - [ ] **TYLKO mikrousługa** - infrastruktura osobno

#### 4.2 CI/CD
- [ ] **GitHub Actions**
  - [ ] Build workflows
  - [ ] Test workflows
  - [ ] Deployment workflows
  - [ ] Security scanning

### 🔍 **5. Observability Module**
**Priorytet: NISKI**

#### 5.1 Logging
- [ ] **Serilog Integration**
  - [ ] Structured logging
  - [ ] Log enrichment
  - [ ] Multiple sinks
  - [ ] Request correlation

#### 5.2 Monitoring
- [ ] **Health Checks**
  - [ ] Application health
  - [ ] Database health
  - [ ] External service health

- [ ] **Metrics**
  - [ ] Prometheus metrics
  - [ ] Custom counters
  - [ ] Performance metrics

### 🎨 **6. Advanced Features**
**Priorytet: NISKI**

#### 6.1 Event Sourcing
- [ ] **Event Store**
  - [ ] Event stream management
  - [ ] Snapshots
  - [ ] Projections
  - [ ] Replay functionality

#### 6.2 CQRS Advanced
- [ ] **Separate Read/Write Models**
  - [ ] Command side optimization
  - [ ] Query side optimization
  - [ ] Eventual consistency
  - [ ] Projection handlers

---

## 🏗️ **ARCHITEKTURA MODUŁÓW**

### Struktura Modułów
```
src/Modules/
├── DDD/                    ✅ GOTOWE
├── Application/            ✅ GOTOWE  
├── Api/                    ❌ DO ZROBIENIA
│   ├── RestApiModule.cs
│   ├── MinimalApiModule.cs
│   └── GrpcModule.cs
├── Tests/                  ❌ DO ZROBIENIA
│   ├── UnitTestModule.cs
│   ├── IntegrationTestModule.cs
│   └── TestUtilitiesModule.cs
├── Infrastructure/         ❌ DO ZROBIENIA
│   ├── PersistenceModule.cs
│   ├── MessagingModule.cs
│   └── ExternalServicesModule.cs
├── Deployment/             ❌ DO ZROBIENIA
│   ├── DockerModule.cs
│   └── KubernetesModule.cs
└── Observability/          ❌ DO ZROBIENIA
    ├── LoggingModule.cs
    └── MonitoringModule.cs
```

### SOLID Principles w Generatorze
- **S** - Single Responsibility: Każdy moduł odpowiada za jedną warstwę
- **O** - Open/Closed: Łatwe dodawanie nowych modułów bez modyfikacji istniejących
- **L** - Liskov Substitution: Wszystkie moduły implementują ITemplateModule
- **I** - Interface Segregation: Oddzielne interfejsy dla różnych typów generatorów
- **D** - Dependency Inversion: Moduły zależą od abstrakcji, nie konkretnych implementacji

---

## 📋 **NASTĘPNE KROKI - PRIORYTET**

### 🔥 **NATYCHMIASTOWE (Ten Sprint)**
1. [x] **ApiModule - REST Controllers** ✅ UKOŃCZONE (2-3 dni)
2. [x] **TestModule - Unit Tests** ✅ UKOŃCZONE (2-3 dni)
3. [x] **Integracja modułów w CLI** ✅ UKOŃCZONE (1 dzień)

### 🚀 **KRÓTKOTERMINOWE (Następny Sprint) - AKTUALNY PRIORYTET**
1. [ ] **🚨 KRYTYCZNY: Stwórz InfrastructureModule** - repositories, DbContext, extensions (2-3 dni)
   - [ ] Stwórz `src/Modules/Infrastructure/InfrastructureModule.cs`
   - [ ] Implementuj generowanie Entity Framework DbContext
   - [ ] Implementuj generowanie Repository pattern
   - [ ] Implementuj konfigurację bazy danych (PostgreSQL, SQL Server, SQLite)
   - [ ] Implementuj dependency injection extensions
   - [ ] Dodaj do CLI registration
2. [ ] **Napraw referencje projektów** - API projekty referencują nieistniejący Infrastructure (1 dzień)
3. [ ] **Test kompilacji** - sprawdź czy wygenerowane mikrousługi się kompilują (0.5 dnia)
4. [ ] **MessagingModule** - RabbitMQ integration (jeśli eventy) (1 dzień)

### 🐳 **CONTAINERIZATION (Sprint 3)**
1. [ ] **DockerModule** - inteligentne generowanie kontenerów:
   - [ ] **microservice.Dockerfile** - zawsze
   - [ ] **rabbitmq.Dockerfile** - jeśli `config.Features.Messaging.Enabled`
   - [ ] **mongodb.Dockerfile** - jeśli `config.Features.Persistence.ReadModel == "mongodb"`
   - [ ] **postgres.Dockerfile** - jeśli `config.Features.Persistence.WriteModel == "postgresql"`
   - [ ] **mysql.Dockerfile** - jeśli `config.Features.Persistence.WriteModel == "mysql"`
   - [ ] **docker-compose.yml** - orkiestracja wszystkich wybranych serwisów

### ☸️ **KUBERNETES (Sprint 4)**
1. [ ] **KubernetesModule** - TYLKO dla mikrousługi:
   - [ ] **Deployment** z replikami i resource limits
   - [ ] **HPA** z CPU/Memory metrics
   - [ ] **Service** (ClusterIP + LoadBalancer)
   - [ ] **ConfigMap** dla appsettings
   - [ ] **Secret** dla connection strings
   - [ ] **Pod** configuration z health checks

### 🎯 **ŚREDNIOTERMINOWE (Sprint 5-6)**
1. [ ] **gRPC Support**
2. [ ] **Advanced CQRS**
3. [ ] **Event Sourcing**

### 🌟 **DŁUGOTERMINOWE (Przyszłość)**
1. [ ] **Event Sourcing**
2. [ ] **Microservice Orchestration**
3. [ ] **Performance Optimization**

---

## 🧪 **SZCZEGÓŁY TESTÓW DO WYGENEROWANIA**

### Unit Tests - Przykłady
```csharp
// Domain Tests
[Fact]
public void Order_WhenCreated_ShouldRaiseDomainEvent()
{
    // Arrange & Act
    var order = new Order(customerId, items);
    
    // Assert
    order.DomainEvents.Should().ContainSingle()
        .Which.Should().BeOfType<OrderCreatedEvent>();
}

// Application Tests  
[Fact]
public async Task CreateOrderHandler_WithValidCommand_ShouldReturnOrderId()
{
    // Arrange
    var command = new CreateOrderCommand(customerId, items);
    var handler = new CreateOrderCommandHandler(mockRepository);
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Should().NotBeEmpty();
    mockRepository.Verify(x => x.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
}

// API Tests
[Fact]
public async Task POST_Orders_WithValidData_ShouldReturn201()
{
    // Arrange
    var request = new CreateOrderRequest { CustomerId = Guid.NewGuid() };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/orders", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

### Integration Tests - Przykłady
```csharp
[Fact]
public async Task OrderWorkflow_EndToEnd_ShouldPersistCorrectly()
{
    // Arrange
    using var scope = factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    
    // Act - Create order via API
    var createResponse = await client.PostAsJsonAsync("/api/orders", createRequest);
    var orderId = await createResponse.Content.ReadFromJsonAsync<Guid>();
    
    // Act - Retrieve order
    var getResponse = await client.GetAsync($"/api/orders/{orderId}");
    var order = await getResponse.Content.ReadFromJsonAsync<OrderDto>();
    
    // Assert
    order.Should().NotBeNull();
    order.Id.Should().Be(orderId);
    
    // Verify in database
    var dbOrder = await dbContext.Orders.FindAsync(orderId);
    dbOrder.Should().NotBeNull();
}
```

---

## 📝 **NOTATKI IMPLEMENTACYJNE**

### Konwencje Nazewnictwa
- **Commands**: `{Action}{Aggregate}Command` (np. `CreateOrderCommand`)
- **Queries**: `Get{Aggregate}Query` (np. `GetOrderByIdQuery`)
- **Handlers**: `{Command/Query}Handler` (np. `CreateOrderCommandHandler`)
- **DTOs**: `{Aggregate}Dto`, `Create{Aggregate}Dto`, `Update{Aggregate}Dto`
- **Tests**: `{ClassUnderTest}Tests` (np. `OrderTests`, `CreateOrderHandlerTests`)

### Struktura Testów
```
tests/
├── {ServiceName}.UnitTests/
│   ├── Domain/
│   │   ├── Entities/
│   │   └── ValueObjects/
│   ├── Application/
│   │   ├── Commands/
│   │   └── Queries/
│   └── Infrastructure/
└── {ServiceName}.IntegrationTests/
    ├── Api/
    ├── Persistence/
    └── EndToEnd/
```

### Test Utilities
- **Builders**: `OrderBuilder`, `CustomerBuilder`
- **Fixtures**: `DatabaseFixture`, `WebApplicationFixture`
- **Mocks**: `MockRepository<T>`, `MockMessageBus`
- **Data**: `TestData.Orders`, `TestData.Customers`

---

## ✅ **KRYTERIA UKOŃCZENIA**

### Definition of Done dla każdego modułu:
- [ ] Kod wygenerowany zgodnie z SOLID principles
- [ ] Testy jednostkowe dla modułu
- [ ] Dokumentacja API (jeśli dotyczy)
- [ ] Przykłady użycia
- [ ] Integracja z CLI
- [ ] Walidacja konfiguracji

### Definition of Done dla całego projektu:
- [ ] Wszystkie moduły zintegrowane
- [ ] Pełna dokumentacja użytkownika
- [ ] Przykłady dla różnych scenariuszy
- [ ] Performance tests
- [ ] Security review
- [ ] NuGet package ready

---

## 🐳 **SZCZEGÓŁY KONTENERYZACJI**

### Docker Files - Inteligentne Generowanie

#### **microservice.Dockerfile** (zawsze generowany)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Api/{ServiceName}.Api/{ServiceName}.Api.csproj", "src/Api/{ServiceName}.Api/"]
COPY ["src/Application/{ServiceName}.Application/{ServiceName}.Application.csproj", "src/Application/{ServiceName}.Application/"]
COPY ["src/Domain/{ServiceName}.Domain/{ServiceName}.Domain.csproj", "src/Domain/{ServiceName}.Domain/"]
COPY ["src/Infrastructure/{ServiceName}.Infrastructure/{ServiceName}.Infrastructure.csproj", "src/Infrastructure/{ServiceName}.Infrastructure/"]
RUN dotnet restore "src/Api/{ServiceName}.Api/{ServiceName}.Api.csproj"
COPY . .
WORKDIR "/src/src/Api/{ServiceName}.Api"
RUN dotnet build "{ServiceName}.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "{ServiceName}.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "{ServiceName}.Api.dll"]
```

#### **docker-compose.yml** (inteligentne komponenty)
```yaml
version: '3.8'

services:
  {servicename}:
    build:
      context: .
      dockerfile: microservice.Dockerfile
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Database={ServiceName}Db;Username=postgres;Password=postgres
    depends_on:
      - postgres
    networks:
      - {servicename}-network

  # Generowane TYLKO jeśli config.Features.Persistence.WriteModel == "postgresql"
  postgres:
    build:
      context: ./docker
      dockerfile: postgres.Dockerfile
    environment:
      POSTGRES_DB: {ServiceName}Db
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - {servicename}-network

  # Generowane TYLKO jeśli config.Features.Persistence.ReadModel == "mongodb"
  mongodb:
    build:
      context: ./docker
      dockerfile: mongodb.Dockerfile
    environment:
      MONGO_INITDB_ROOT_USERNAME: admin
      MONGO_INITDB_ROOT_PASSWORD: admin
      MONGO_INITDB_DATABASE: {ServiceName}ReadDb
    ports:
      - "27017:27017"
    volumes:
      - mongodb_data:/data/db
    networks:
      - {servicename}-network

  # Generowane TYLKO jeśli config.Features.Messaging.Enabled == true
  rabbitmq:
    build:
      context: ./docker
      dockerfile: rabbitmq.Dockerfile
    environment:
      RABBITMQ_DEFAULT_USER: admin
      RABBITMQ_DEFAULT_PASS: admin
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    networks:
      - {servicename}-network

volumes:
  postgres_data:
  mongodb_data:
  rabbitmq_data:

networks:
  {servicename}-network:
    driver: bridge
```

### Kubernetes Manifests - TYLKO Mikrousługa

#### **deployment.yaml**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {servicename}-deployment
  labels:
    app: {servicename}
spec:
  replicas: 3
  selector:
    matchLabels:
      app: {servicename}
  template:
    metadata:
      labels:
        app: {servicename}
    spec:
      containers:
      - name: {servicename}
        image: {servicename}:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: {servicename}-secrets
              key: connection-string
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
```

#### **hpa.yaml**
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: {servicename}-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: {servicename}-deployment
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
```

### Logika Generowania

```csharp
public class DockerModule : ITemplateModule
{
    public async Task GenerateAsync(GenerationContext context)
    {
        var config = context.Configuration;
        
        // Zawsze generuj mikrousługę
        await GenerateMicroserviceDockerfile(context);
        
        // Inteligentnie generuj infrastrukturę
        var services = new List<string> { "microservice" };
        
        if (config.Features?.Messaging?.Enabled == true)
        {
            await GenerateRabbitMQDockerfile(context);
            services.Add("rabbitmq");
        }
        
        if (config.Features?.Persistence?.WriteModel == "postgresql")
        {
            await GeneratePostgreSQLDockerfile(context);
            services.Add("postgres");
        }
        
        if (config.Features?.Persistence?.ReadModel == "mongodb")
        {
            await GenerateMongoDBDockerfile(context);
            services.Add("mongodb");
        }
        
        // Generuj docker-compose z wybranymi serwisami
        await GenerateDockerCompose(context, services);
    }
}
```

---

**Ostatnia aktualizacja**: 2024-12-08
**Status**: 🚧 W trakcie rozwoju - Faza 2: API & Tests → Faza 3: Infrastructure & Containers → Faza 3: Infrastructure & Containers 