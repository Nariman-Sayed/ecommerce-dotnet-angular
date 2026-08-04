# System Architecture

```mermaid
flowchart TB
  Client["Angular Client (Browser)"]

  subgraph API["ASP.NET Core 8 Web API"]
    Ctrl["Controllers"]
    Mid["Middleware (Exception, Rate Limit, CORS)"]
    Svc["Services (Order, Payment, Email, Token)"]
    Repo["Repositories + Unit of Work"]
  end

  DB[("SQL Server (EF Core)")]
  Redis[("Redis (Basket Cache)")]
  Stripe["Stripe API (Payments + Webhook)"]
  SMTP["Email Service (Verification/Reset)"]

  Client -->|HTTPS / JWT Cookie| Mid
  Mid --> Ctrl
  Ctrl --> Svc
  Svc --> Repo
  Repo --> DB
  Svc -->|Basket ops| Redis
  Svc -->|Create Payment Intent| Stripe
  Stripe -->|webhook callback| Ctrl
  Svc -->|Send email| SMTP
```

This diagram shows my system at a container level: the Angular client,
the API and its internal layers, and the external systems it depends
on. I confirmed each component directly in my own code: SQL Server
through EF Core, Redis through the StackExchange.Redis package
registered in infrastructureRegisteration.cs, and the Stripe integration
in PaymentService.cs and PaymentsController.cs.