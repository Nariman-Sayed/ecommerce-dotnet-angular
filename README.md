# Ecom API

A production oriented e-commerce backend built with ASP.NET Core 8. I am
building this project by following a guided learning course as a
starting structure, and extending it toward production standards
through a phased, reviewed development process.

## About This Project

I am building this project by following a guided learning course,
ASP.NET Core 8 with Angular, and extending it toward a production level
system. See docs/requirements.md for the full scope and known gaps I am
tracking.

## Development Process

I write and test all code locally before pushing. I am following a
phased development approach with code review from experienced software
engineers at each milestone, covering architecture, security, frontend,
and DevOps. I deliver each phase as a separate pull request with its
own review cycle. See docs/roadmap.md for my phased plan.

## Tech Stack

ASP.NET Core 8 Web API. Entity Framework Core with SQL Server. ASP.NET
Core Identity with JWT authentication. Redis, using StackExchange.Redis,
for basket caching. Stripe for payment processing. AutoMapper for DTO
mapping.

## Project Structure

The solution follows a layered architecture across three projects.
Ecom.Core holds entities, DTOs, service interfaces, and repository
interfaces. Ecom.infrastructure holds EF Core configuration, migrations,
repository implementations, and service implementations for orders,
payments, email, and token generation. Ecom.API holds controllers,
middleware, and request and response mapping.

## Documentation

Further documentation lives in the docs folder. requirements.md covers
business rules, actors, and known gaps verified from my source code.
erd.md is the entity relationship diagram. architecture.md is the
system architecture diagram. api-contract.md is the endpoint reference
with authorization status. definition-of-done.md covers branching and
review conventions. roadmap.md is my phased development plan.

## Getting Started

I will add setup instructions once configuration and secrets management
are finalized. See the Configuration and Secrets phase in
docs/roadmap.md.