# 📨 Messaging & Events Implementation Plan

## Overview
This document outlines the implementation plan for adding event-driven architecture and messaging capabilities to MicroserviceKit using open-source solutions.

## 🎯 Technology Choices

### Primary Option: MassTransit + RabbitMQ
**Why MassTransit?**
- ✅ Most mature and widely adopted in .NET ecosystem
- ✅ Excellent documentation and community support
- ✅ Built-in patterns: Publish/Subscribe, Request/Response, Saga
- ✅ Automatic retry policies and error handling
- ✅ Perfect integration with ASP.NET Core and EF Core
- ✅ Apache 2.0 license (fully open source)

### Alternative Option: Wolverine + RabbitMQ
**Why Wolverine?**
- ✅ Modern approach with source generators
- ✅ Built-in CQRS and messaging
- ✅ Excellent performance
- ✅ Part of "Critter Stack" (.NET ecosystem)
- ✅ MIT license (fully open source)

### Outbox Pattern: CAP (DotNetCore.CAP)
**Why CAP?**
- ✅ Outbox pattern out-of-the-box
- ✅ Distributed transaction support
- ✅ Built-in dashboard
- ✅ MIT license (fully open source)

## 🏗️ Architecture Design

### 1. Message Types
```csharp
// Domain Events (Published within bounded context)
public record OrderCreated(Guid OrderId, Guid CustomerId, decimal Amount, DateTime CreatedAt);

// Integration Events (Published across bounded contexts)
public record CustomerRegistered(Guid CustomerId, string Email, DateTime RegisteredAt);

// Commands (Direct service communication)
public record ProcessPayment(Guid OrderId, decimal Amount, string PaymentMethod);
```

### 2. Publisher Integration
```csharp
// In Domain Aggregates
public class Order : AggregateRoot
{
    public void Create(Guid customerId, decimal amount)
    {
        // Business logic
        AddDomainEvent(new OrderCreated(Id, customerId, amount, DateTime.UtcNow));
    }
}

// In Application Layer
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand>
{
    public async Task Handle(CreateOrderCommand command)
    {
        var order = Order.Create(command.CustomerId, command.Amount);
        await _repository.AddAsync(order);
        
        // Events published automatically via outbox pattern
        await _unitOfWork.SaveChangesAsync();
    }
}
```

### 3. Consumer Implementation
```csharp
// MassTransit Consumer
public class OrderCreatedConsumer : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var @event = context.Message;
        // Handle the event
        await _emailService.SendOrderConfirmation(@event.CustomerId, @event.OrderId);
    }
}

// Wolverine Consumer (alternative)
public static class OrderEventHandlers
{
    public static async Task Handle(OrderCreated @event, IEmailService emailService)
    {
        await emailService.SendOrderConfirmation(@event.CustomerId, @event.OrderId);
    }
}
```

## 📁 Module Structure

### MessagingModule Directory Structure
```
src/Modules/Messaging/
├── MessagingModule.cs
├── Messaging.csproj
├── Templates/
│   ├── MassTransit/
│   │   ├── Startup.cs.template
│   │   ├── Consumer.cs.template
│   │   ├── Publisher.cs.template
│   │   └── Configuration.cs.template
│   ├── Wolverine/
│   │   ├── Startup.cs.template
│   │   ├── Handler.cs.template
│   │   └── Configuration.cs.template
│   ├── CAP/
│   │   ├── Startup.cs.template
│   │   ├── Subscriber.cs.template
│   │   └── Configuration.cs.template
│   └── Common/
│       ├── Events.cs.template
│       ├── Commands.cs.template
│       └── docker-compose.rabbitmq.yml.template
└── Configuration/
    └── MessagingConfiguration.cs
```

## 🛠️ Implementation Steps

### Step 1: Create MessagingModule (Day 1)
1. **Create module structure**
   ```bash
   mkdir -p src/Modules/Messaging/Templates/{MassTransit,Wolverine,CAP,Common}
   mkdir -p src/Modules/Messaging/Configuration
   ```

2. **Implement MessagingModule.cs**
   ```csharp
   public class MessagingModule : ITemplateModule
   {
       public string Name => "Messaging";
       public string Description => "Generates event-driven messaging with publishers and consumers";
       
       public bool IsEnabled(TemplateConfiguration config)
       {
           return config.Features.Messaging.Enabled;
       }
       
       public async Task GenerateAsync(GenerationContext context)
       {
           var messagingConfig = context.Configuration.Features.Messaging;
           
           switch (messagingConfig.Provider)
           {
               case MessagingProvider.MassTransit:
                   await GenerateMassTransitAsync(context);
                   break;
               case MessagingProvider.Wolverine:
                   await GenerateWolverineAsync(context);
                   break;
               case MessagingProvider.CAP:
                   await GenerateCAPAsync(context);
                   break;
           }
           
           await GenerateCommonAsync(context);
       }
   }
   ```

3. **Add MessagingConfiguration**
   ```csharp
   public class MessagingConfiguration
   {
       public bool Enabled { get; set; } = false;
       public MessagingProvider Provider { get; set; } = MessagingProvider.MassTransit;
       public bool EnableOutboxPattern { get; set; } = true;
       public bool EnableEventSourcing { get; set; } = false;
       public RabbitMQConfiguration RabbitMQ { get; set; } = new();
   }
   
   public enum MessagingProvider
   {
       MassTransit,
       Wolverine,
       CAP
   }
   ```

### Step 2: Generate Templates (Day 2)

1. **MassTransit Templates**
   - Startup configuration with RabbitMQ
   - Consumer base classes
   - Publisher integration with Domain Events
   - Outbox pattern with EF Core

2. **Wolverine Templates**
   - Host configuration
   - Message handlers
   - Event publishing integration

3. **CAP Templates**
   - Service configuration
   - Subscriber implementations
   - Publisher with transaction support

4. **Common Templates**
   - Domain event base classes
   - Integration event schemas
   - RabbitMQ Docker Compose setup

### Step 3: Integration with Existing Modules (Day 3)

1. **Domain Module Integration**
   - Add event publishing to AggregateRoot
   - Generate domain events for entities
   - Integrate with repository pattern

2. **Application Module Integration**
   - Add event handlers to CQRS
   - Integrate with command/query handlers
   - Add outbox pattern support

3. **Infrastructure Module Integration**
   - Add messaging infrastructure
   - Configure message persistence
   - Add health checks for message brokers

### Step 4: Testing & Documentation (Day 4)

1. **Generate Integration Tests**
   - Message publishing tests
   - Consumer processing tests
   - End-to-end event flow tests

2. **Update Documentation**
   - Messaging configuration guide
   - Event-driven architecture patterns
   - Deployment with RabbitMQ

## 📋 Configuration Examples

### Template Configuration
```json
{
  "serviceName": "OrderService",
  "features": {
    "messaging": {
      "enabled": true,
      "provider": "MassTransit",
      "enableOutboxPattern": true,
      "enableEventSourcing": false,
      "rabbitMQ": {
        "host": "localhost",
        "virtualHost": "/",
        "username": "guest",
        "password": "guest"
      }
    }
  }
}
```

### Generated appsettings.json
```json
{
  "Messaging": {
    "Provider": "MassTransit",
    "RabbitMQ": {
      "Host": "localhost",
      "VirtualHost": "/",
      "Username": "guest",
      "Password": "guest"
    },
    "OutboxPattern": {
      "Enabled": true,
      "CleanupInterval": "00:05:00"
    }
  }
}
```

## 🚀 Benefits

### For Developers
- **Easy Setup**: One command generates complete messaging infrastructure
- **Best Practices**: Built-in outbox pattern, error handling, retry policies
- **Flexibility**: Choose between MassTransit, Wolverine, or CAP
- **Testing**: Generated integration tests for message flows

### For Architecture
- **Decoupling**: Services communicate via events, not direct calls
- **Scalability**: Asynchronous processing and competing consumers
- **Reliability**: Outbox pattern ensures message delivery
- **Observability**: Built-in monitoring and health checks

### For Operations
- **Docker Support**: RabbitMQ containers included
- **Kubernetes Ready**: Messaging infrastructure manifests
- **Monitoring**: Health checks and metrics
- **Documentation**: Complete setup and usage guides

## 🔄 Migration Path

### From Synchronous to Event-Driven
1. **Phase 1**: Add messaging module to existing services
2. **Phase 2**: Identify integration points for events
3. **Phase 3**: Replace synchronous calls with events
4. **Phase 4**: Add saga patterns for complex workflows

### Backward Compatibility
- Messaging is optional (disabled by default)
- Existing services continue to work without changes
- Gradual adoption possible service by service

## 📊 Success Metrics

### Technical Metrics
- ✅ Message throughput and latency
- ✅ Consumer processing times
- ✅ Error rates and retry success
- ✅ Queue depths and processing backlogs

### Developer Experience
- ✅ Time to add new event types
- ✅ Ease of testing message flows
- ✅ Documentation completeness
- ✅ Community adoption and feedback

This implementation will make MicroserviceKit a complete solution for modern, event-driven microservices architecture! 🎉 