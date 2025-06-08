# 🏗️ Infrastructure Layer - Kiedy używać?

## 📊 Porównanie architektur

### 🟢 MINIMAL - BEZ Infrastructure Layer

**Kiedy używać:**
- ✅ Proste CRUD operacje
- ✅ 1-2 deweloperów
- ✅ Jedna baza danych
- ✅ Brak external services
- ✅ Szybki prototyp/MVP

**Struktura:**
```
BlogService/
├── BlogService.sln
├── src/BlogService/
│   ├── Domain/
│   │   ├── Post.cs
│   │   └── IPostRepository.cs
│   ├── Application/
│   │   ├── CreatePost.cs
│   │   └── GetPosts.cs
│   ├── Data/
│   │   ├── BlogDbContext.cs
│   │   └── PostRepository.cs
│   ├── Controllers/
│   │   └── PostsController.cs
│   └── Program.cs
└── tests/BlogService.Tests/
```

**Przykład kodu:**
```csharp
// Data/PostRepository.cs - wszystko w jednym projekcie
public class PostRepository : IPostRepository
{
    private readonly BlogDbContext _context;
    
    public PostRepository(BlogDbContext context)
    {
        _context = context;
    }
    
    public async Task<Post> CreateAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }
}

// Program.cs - prosta konfiguracja
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseSqlite("Data Source=blog.db"));
builder.Services.AddScoped<IPostRepository, PostRepository>();
```

---

### 🟡 STANDARD - Z Infrastructure Layer

**Kiedy używać:**
- 🔧 2+ external services
- 🔧 Caching (Redis)
- 🔧 Messaging (RabbitMQ)
- 🔧 2-5 deweloperów
- 🔧 Business logic w persistence

**Struktura:**
```
OrderService/
├── OrderService.sln
├── src/
│   ├── OrderService.Api/
│   ├── OrderService.Application/
│   ├── OrderService.Domain/
│   └── OrderService.Infrastructure/  # ← POTRZEBNY!
│       ├── Persistence/
│       │   ├── OrderDbContext.cs
│       │   └── OrderRepository.cs
│       ├── ExternalServices/
│       │   ├── PaymentGateway.cs
│       │   └── EmailService.cs
│       ├── Caching/
│       │   └── RedisCacheService.cs
│       └── Extensions/
│           └── ServiceCollectionExtensions.cs
└── tests/
```

**Dlaczego Infrastructure:**
```csharp
// Infrastructure/ExternalServices/PaymentGateway.cs
public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly HttpClient _httpClient;
    private readonly IPollyRetryPolicy _retryPolicy;
    private readonly ILogger<PaymentGatewayService> _logger;
    
    // Complex external service logic with resilience patterns
    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var response = await _httpClient.PostAsJsonAsync("/payments", request);
            // Complex error handling, logging, etc.
        });
    }
}

// Infrastructure/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddInfrastructure(this IServiceCollection services)
{
    services.AddDbContext<OrderDbContext>();
    services.AddStackExchangeRedisCache();
    services.AddHttpClient<PaymentGatewayService>();
    services.AddPolly(); // Resilience patterns
    services.AddMassTransit(); // Messaging
    return services;
}
```

---

### 🔴 ENTERPRISE - Infrastructure WYMAGANY

**Kiedy używać:**
- 🚀 5+ external services
- 🚀 CQRS z separate read/write models
- 🚀 Event Sourcing
- 🚀 Complex messaging patterns
- 🚀 5+ deweloperów
- 🚀 Microservices ecosystem

**Struktura:**
```
PaymentService/
├── PaymentService.sln
├── src/
│   ├── PaymentService.Api/
│   ├── PaymentService.Application/
│   ├── PaymentService.Domain/
│   └── PaymentService.Infrastructure/  # ← KLUCZOWY!
│       ├── Persistence/
│       │   ├── Write/
│       │   │   ├── WriteDbContext.cs
│       │   │   └── PaymentWriteRepository.cs
│       │   ├── Read/
│       │   │   ├── MongoDbContext.cs
│       │   │   └── PaymentReadRepository.cs
│       │   └── EventStore/
│       │       └── EventStoreRepository.cs
│       ├── ExternalServices/
│       │   ├── PaymentGateway/
│       │   ├── FraudDetection/
│       │   ├── NotificationService/
│       │   └── AuditService/
│       ├── Messaging/
│       │   ├── RabbitMQ/
│       │   ├── EventHandlers/
│       │   └── Sagas/
│       ├── Caching/
│       │   ├── Redis/
│       │   └── CacheStrategies/
│       └── BackgroundJobs/
│           ├── Hangfire/
│           └── Jobs/
└── tests/
```

**Dlaczego Infrastructure WYMAGANY:**
```csharp
// Infrastructure/Persistence/Write/PaymentWriteRepository.cs
public class PaymentWriteRepository : IPaymentWriteRepository
{
    private readonly WriteDbContext _writeContext;
    private readonly IEventStore _eventStore;
    private readonly IOutboxService _outboxService;
    
    public async Task SaveAsync(Payment payment)
    {
        using var transaction = await _writeContext.Database.BeginTransactionAsync();
        
        // Save to write model
        _writeContext.Payments.Update(payment);
        
        // Save events to event store
        await _eventStore.SaveEventsAsync(payment.Id, payment.GetUncommittedEvents());
        
        // Save to outbox for eventual consistency
        await _outboxService.PublishAsync(payment.GetUncommittedEvents());
        
        await _writeContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}

// Infrastructure/ExternalServices/PaymentGateway/StripePaymentGateway.cs
public class StripePaymentGateway : IPaymentGateway
{
    private readonly HttpClient _httpClient;
    private readonly ICircuitBreaker _circuitBreaker;
    private readonly IBulkhead _bulkhead;
    private readonly IRetryPolicy _retryPolicy;
    private readonly IMetrics _metrics;
    
    // Complex resilience patterns, monitoring, etc.
}
```

---

## 🎯 **Automatyczna decyzja w generatorze:**

```json
{
  "microserviceName": "BlogService",
  "architecture": { "level": "auto" },
  "features": {
    "externalServices": { "enabled": false },
    "messaging": { "enabled": false },
    "database": { 
      "cache": { "enabled": false },
      "readModel": { "provider": "same" }
    }
  }
}
```
**→ Wynik: MINIMAL (bez Infrastructure)**

```json
{
  "microserviceName": "OrderService", 
  "architecture": { "level": "auto" },
  "features": {
    "externalServices": { 
      "enabled": true,
      "services": ["PaymentGateway", "EmailService"]
    },
    "database": { 
      "cache": { "enabled": true, "provider": "redis" }
    }
  }
}
```
**→ Wynik: STANDARD (z Infrastructure)**

```json
{
  "microserviceName": "PaymentService",
  "architecture": { "level": "auto" },
  "features": {
    "externalServices": { 
      "enabled": true,
      "services": ["PaymentGateway", "FraudDetection", "Notifications", "Audit", "Compliance"]
    },
    "messaging": { "enabled": true, "patterns": ["outbox", "saga"] },
    "database": { 
      "readModel": { "provider": "mongodb" },
      "eventStore": { "enabled": true },
      "cache": { "enabled": true }
    }
  }
}
```
**→ Wynik: ENTERPRISE (z Infrastructure)**

---

## 📈 **Migration Path:**

1. **Start MINIMAL** - szybki prototyp
2. **Gdy dodajesz external service** → upgrade do STANDARD
3. **Gdy potrzebujesz messaging/CQRS** → upgrade do ENTERPRISE

**Generator automatycznie wykryje kiedy Infrastructure jest potrzebny!** 🎯 