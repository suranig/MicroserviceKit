# Headless CMS Microservices Architecture - Plan Kompleksowy

## 🎯 **Wizja Systemu**

### **Cel:**
Headless CMS dla wielu redakcji (se.pl, polskieradio.pl, eska.pl, gazetawyborcza.pl) z architekturą mikroserwisową, DDD, CQRS i event-driven design.

### **Kluczowe Założenia:**
- **1 domena = 1 tenant** (multi-tenancy)
- **Gradualna migracja** z monolitu
- **Event-driven architecture** z propagacją do starej bazy
- **Read models** dla frontendów (Next.js microfrontends)
- **Centralne zarządzanie tożsamością** (OAuth dla zewnętrznych, JWT dla użytkowników CMS)

---

## 🏗️ **Architektura Mikroserwisów**

### **1. Core CMS Services (Write Side)**

#### **1.1 Article Service**
```
ArticleService/
├── Domain/
│   ├── Aggregates/
│   │   ├── Article.cs
│   │   ├── ArticleBlock.cs (text, quote, image, video, embed)
│   │   └── ArticleVersion.cs
│   ├── ValueObjects/
│   │   ├── ArticleStatus.cs
│   │   ├── BlockType.cs
│   │   └── TenantId.cs
│   └── Events/
│       ├── ArticleCreated.cs
│       ├── ArticlePublished.cs
│       ├── ArticleBlockAdded.cs
│       └── ArticleUpdated.cs
├── Application/
│   ├── Commands/
│   │   ├── CreateArticle/
│   │   ├── PublishArticle/
│   │   └── AddArticleBlock/
│   └── Queries/
├── Infrastructure/
│   ├── Persistence/
│   ├── ExternalServices/
│   │   ├── ImageServiceAdapter.cs
│   │   ├── VideoServiceAdapter.cs
│   │   └── EmbedServiceAdapter.cs
│   └── Messaging/
└── Api/
```

**Wzorce:** CQRS + Event Sourcing + Saga (dla bloków zewnętrznych)

**ArticleBlock z External References:**
```csharp
public abstract class ArticleBlock
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public BlockType Type { get; set; }
    public TenantId TenantId { get; set; }
    
    // External references
    public string? ExternalService { get; set; } // "ImageService", "VideoService"
    public string? ExternalId { get; set; }      // UID from external service
    public string? ExternalType { get; set; }    // "image", "video", "embed"
    
    public Dictionary<string, object> Data { get; set; }
}
```

#### **1.2 Page Service**
```
PageService/
├── Domain/
│   ├── Aggregates/
│   │   ├── Page.cs
│   │   ├── PageLayout.cs
│   │   ├── PageMetadata.cs
│   │   ├── PageComponent.cs (widgety/komponenty)
│   │   └── PublishingPage.cs (strona wydawnicza)
│   └── Events/
├── Application/
│   ├── Commands/
│   │   ├── CreatePage/
│   │   ├── UpdateLayout/
│   │   ├── AddComponent/
│   │   └── PublishPage/
│   └── Queries/
└── Infrastructure/
```

**Wzorce:** CQRS + Event Sourcing

**PublishingPage** - strona do zarządzania która zaciąga układ z Page i pozwala na modyfikacje

#### **1.3 Tag Taxonomy Service**
```
TagTaxonomyService/
├── Domain/
│   ├── Aggregates/
│   │   ├── Tag.cs
│   │   ├── TagGroup.cs
│   │   └── TagAssignment.cs
│   └── Events/
├── Application/
└── Infrastructure/
```

**Wzorce:** Flat Data + CQRS

#### **1.4 Category Taxonomy Service**
```
CategoryTaxonomyService/
├── Domain/
│   ├── Aggregates/
│   │   ├── Category.cs
│   │   ├── CategoryTree.cs
│   │   └── CategoryHierarchy.cs
│   └── Events/
├── Application/
└── Infrastructure/
```

**Wzorce:** Hierarchical Data + CQRS

#### **1.5 Media Service**
```
MediaService/
├── Domain/
│   ├── Aggregates/
│   │   ├── Media.cs
│   │   ├── Image.cs
│   │   ├── Video.cs
│   │   └── Document.cs
│   └── Events/
└── Application/
```

**Wzorce:** Blob Storage + Event Sourcing

### **2. Read Model Services (Read Side)**

#### **2.1 Content Read Service**
```
ContentReadService/
├── Application/
│   ├── Projections/
│   │   ├── ArticleProjection.cs
│   │   ├── PageProjection.cs
│   │   └── TaxonomyProjection.cs
│   └── Queries/
├── Infrastructure/
│   ├── Persistence/ (MongoDB)
│   └── Cache/ (Redis)
└── Api/
    ├── Controllers/
    │   ├── ArticlesController.cs
    │   ├── PagesController.cs
    │   └── SearchController.cs
    └── DTOs/
```

**Wzorce:** CQRS Read Side + Event Projections

#### **2.2 Search Service**
```
SearchService/
├── Application/
│   ├── Indexers/
│   └── Queries/
├── Infrastructure/
│   └── Elasticsearch/
└── Api/
```

**Wzorce:** Search Indexing + Event-Driven

### **3. Infrastructure Services**

#### **3.1 Identity Service**
```
IdentityService/
├── Domain/
│   ├── Aggregates/
│   │   ├── User.cs
│   │   ├── Role.cs
│   │   ├── Permission.cs
│   │   └── OAuthClient.cs
│   └── Events/
├── Application/
└── Infrastructure/
    ├── JWT/ (dla użytkowników CMS)
    ├── OAuth/ (dla zewnętrznych aplikacji)
    └── RBAC/
```

**Wzorce:** OAuth2 + JWT + RBAC

**Security Architecture:**
```
External Apps/Frontend → OAuth2 → API Gateway → JWT → Internal Services
CMS Users → JWT → Internal Services (bezpośrednio)
```

#### **3.2 Tenant Management Service**
```
TenantService/
├── Domain/
│   ├── Aggregates/
│   │   ├── Tenant.cs
│   │   ├── TenantConfiguration.cs
│   │   └── TenantDomain.cs
│   ├── ValueObjects/
│   │   ├── TenantId.cs
│   │   ├── DomainName.cs
│   │   └── TenantStatus.cs
│   └── Events/
│       ├── TenantCreated.cs
│       ├── TenantDomainAdded.cs
│       └── TenantConfigurationUpdated.cs
├── Application/
└── Infrastructure/
```

**Wzorce:** Configuration Management + Multi-tenancy

#### **3.3 API Gateway**
```
ApiGateway/
├── Middleware/
│   ├── Authentication/
│   ├── Authorization/
│   ├── RateLimiting/
│   ├── TenantResolution/
│   └── HealthChecks/
├── Routing/
└── Aggregation/
```

**Wzorce:** API Gateway + Circuit Breaker

#### **3.4 Event Store**
```
EventStore/
├── Application/
│   ├── EventHandlers/
│   └── Projections/
├── Infrastructure/
│   └── Persistence/
└── Api/
```

**Wzorce:** Event Store + Event Sourcing

### **4. Migration Services**

#### **4.1 Legacy Sync Service**
```
LegacySyncService/
├── Application/
│   ├── Adapters/
│   │   ├── ArticleAdapter.cs
│   │   ├── PageAdapter.cs
│   │   └── UserAdapter.cs
│   └── SyncHandlers/
├── Infrastructure/
│   └── LegacyDatabase/
└── Api/
```

**Wzorce:** Adapter Pattern + Event-Driven Sync

#### **4.2 BPC Service (Business Process Control)**
```
BpcService/
├── Domain/
│   ├── Aggregates/
│   │   ├── Workflow.cs
│   │   ├── Process.cs
│   │   ├── Task.cs
│   │   └── ExternalProvider.cs
│   └── Events/
├── Application/
│   ├── Commands/
│   │   ├── StartVideoProcessing/
│   │   ├── SyncToExternalProvider/
│   │   └── TrackProcessingStatus/
│   └── Queries/
└── Infrastructure/
    ├── ExternalProviders/
    │   ├── YouTubeProvider.cs
    │   ├── VimeoProvider.cs
    │   └── AudioProvider.cs
    └── Persistence/
```

**Wzorce:** Workflow Engine + Saga Pattern + External Provider Integration

---

## 🔄 **Event Flow Architecture**

### **Event Schema:**
```json
{
  "eventId": "uuid",
  "eventType": "ArticlePublished",
  "aggregateId": "article-uuid",
  "tenantId": "se.pl",
  "timestamp": "2024-01-01T10:00:00Z",
  "version": 1,
  "data": {
    "articleId": "uuid",
    "title": "Article Title",
    "status": "Published",
    "publishedAt": "2024-01-01T10:00:00Z"
  }
}
```

### **Event Flow:**
1. **Article Service** → `ArticlePublished` event
2. **Event Store** → stores event
3. **Content Read Service** → updates MongoDB projection
4. **Search Service** → updates Elasticsearch index
5. **Legacy Sync Service** → updates old database
6. **Cache Service** → invalidates Redis cache
7. **Frontend** → receives cache invalidation event

---

## 🎨 **Block System Architecture**

### **Block Types:**
```csharp
public abstract class ArticleBlock
{
    public Guid Id { get; set; }
    public int Order { get; set; }
    public BlockType Type { get; set; }
    public TenantId TenantId { get; set; }
    
    // External references
    public string? ExternalService { get; set; } // "ImageService", "VideoService"
    public string? ExternalId { get; set; }      // UID from external service
    public string? ExternalType { get; set; }    // "image", "video", "embed"
    
    public Dictionary<string, object> Data { get; set; }
}

public class TextBlock : ArticleBlock
{
    public string Content { get; set; }
    public string Format { get; set; } // markdown, html, plain
}

public class ImageBlock : ArticleBlock
{
    public string AltText { get; set; }
    public string Caption { get; set; }
    public ImageMetadata? Metadata { get; set; }
}

public class EmbedBlock : ArticleBlock
{
    public string Provider { get; set; } // twitter, instagram, youtube
    public string EmbedCode { get; set; }
    public string ExternalId { get; set; }
}
```

### **External Service Integration:**
- **Image Service** → handles image uploads, processing
- **Video Service** → handles video uploads, transcoding
- **Embed Service** → handles external embeds (Twitter, Instagram)

---

## 🔐 **Security & Multi-Tenancy**

### **Tenant Isolation:**
```csharp
public class TenantContext
{
    public string TenantId { get; set; }
    public string Domain { get; set; }
    public TenantConfig Config { get; set; }
}
```

### **JWT Claims (CMS Users):**
```json
{
  "sub": "user-uuid",
  "tenant": "se.pl",
  "roles": ["editor", "publisher"],
  "permissions": ["article.create", "article.publish"]
}
```

### **OAuth2 (External Applications):**
```json
{
  "client_id": "frontend-app",
  "scope": "read:articles write:articles",
  "grant_type": "client_credentials"
}
```

---

## 🚀 **CLI Architecture - Nowe Podejście**

### **Template-Driven Generation:**
```bash
# Generowanie nowego mikroserwisu
microkit generate ArticleService --template cqrs-event-sourcing

# Generowanie z customizacją
microkit generate PageService --template cqrs-event-sourcing --customize

# Listowanie dostępnych wzorców
microkit list patterns

# Szczegółowy opis szablonu
microkit describe cqrs-event-sourcing

# Lista z opisami
microkit list patterns --detailed
```

### **Wzorce Szablonów:**
1. **`cqrs-event-sourcing.json`** - CQRS + Event Sourcing
2. **`cqrs-saga.json`** - CQRS + Saga Pattern
3. **`event-store.json`** - Event Store Service
4. **`read-model.json`** - Read Model Service
5. **`api-gateway.json`** - API Gateway
6. **`identity-service.json`** - Identity & Auth (OAuth + JWT)
7. **`tenant-service.json`** - Tenant Management
8. **`legacy-sync.json`** - Legacy Integration
9. **`bpc-service.json`** - Business Process Control
10. **`tag-taxonomy.json`** - Tag Taxonomy Service
11. **`category-taxonomy.json`** - Category Taxonomy Service

### **Template Metadata:**
```json
{
  "templateType": "cqrs-event-sourcing",
  "name": "CQRS with Event Sourcing",
  "description": "Command Query Responsibility Segregation with Event Sourcing pattern. Use for write-heavy services with complex business logic.",
  "whenToUse": [
    "Services with complex business rules",
    "Audit trail requirements",
    "Event-driven architecture",
    "Write-heavy workloads"
  ],
  "technologies": [
    "Entity Framework Core",
    "EventStore",
    "MassTransit",
    "PostgreSQL"
  ],
  "complexity": "enterprise",
  "estimatedTime": "20 minutes"
}
```

### **Parametryzacja:**
```json
{
  "serviceName": "ArticleService",
  "template": "cqrs-event-sourcing",
  "customizations": {
    "aggregates": ["Article", "ArticleBlock", "ArticleVersion"],
    "externalServices": ["ImageService", "VideoService"],
    "events": ["ArticleCreated", "ArticlePublished"],
    "database": "postgresql",
    "cache": "redis",
    "messaging": "rabbitmq"
  }
}
```

---

## 📋 **Plan Implementacji**

### **Faza 1: Foundation (2-3 tygodnie)**
1. ✅ Przepisanie CLI z nowymi wzorcami
2. ✅ Tenant Management Service
3. ✅ Identity Service (JWT + OAuth)
4. ✅ API Gateway
5. ✅ Event Store
6. ✅ Base Infrastructure (Docker, Monitoring)

### **Faza 2: Core CMS (4-6 tygodni)**
1. ✅ Article Service (CQRS + Event Sourcing)
2. ✅ Tag Taxonomy Service
3. ✅ Category Taxonomy Service
4. ✅ Media Service
5. ✅ Content Read Service
6. ✅ Search Service

### **Faza 3: Advanced Features (3-4 tygodnie)**
1. ✅ Page Service
2. ✅ BPC Service
3. ✅ Legacy Sync Service
4. ✅ Advanced Block System

### **Faza 4: Migration & Polish (2-3 tygodnie)**
1. ✅ Migration tools
2. ✅ Performance optimization
3. ✅ Monitoring & alerting
4. ✅ Documentation

---

## 🛠️ **Technologie**

### **Core:**
- **.NET 8** + ASP.NET Core
- **Entity Framework Core** (PostgreSQL)
- **MongoDB** (Read Models)
- **Redis** (Caching)
- **RabbitMQ** + **MassTransit** (Messaging)

### **External:**
- **Elasticsearch** (Search)
- **MinIO** (Object Storage)
- **Hangfire** (Background Jobs)

### **Infrastructure:**
- **Docker** + **Kubernetes**
- **Prometheus** + **Grafana**
- **Jaeger** (Distributed Tracing)

---
