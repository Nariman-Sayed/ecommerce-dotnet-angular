# E-Commerce Platform Requirements

I am building this project by following a guided learning course
(ASP.NET Core 8 with Angular) as a starting structure, while extending
it toward production standards as I go. This document reflects what I
have actually implemented so far, confirmed against my own code, not
what I assume the course will eventually cover.

## Project Goal

I started this project as a tutorial based learning path, but my target
is to reach a production level e-commerce system that can serve as a
core portfolio project for remote Software Engineer roles.

Every gap I find in my own code, whether in security, testing, or any
other area, is not something I accept just because it came from the
tutorial. I am tracking each gap as a requirement to resolve before the
project is complete. See the Security Findings section below and
docs/roadmap.md for my phased plan from tutorial baseline to production
readiness.

My working definition of production level for this project:

No unauthenticated write access to protected resources.
No secrets committed to source control.
Automated testing, unit and integration, on core flows.
A CI/CD pipeline, containerization, structured logging, and health checks.
Documented architecture decisions for every non trivial design choice.

## Actors

### Guest (unauthenticated)

Can browse products, filter and sort them, search by keyword, view
product details, and add items to the basket.

### Registered User

Has all Guest capabilities, plus the ability to register, log in and
log out, manage a shipping address, complete checkout by selecting a
delivery method and paying through Stripe, view past orders, and rate
a product.

### Admin

Not implemented as of this document's writing, August 2026. Product and
Category CRUD endpoints currently have no role restriction. When I
implement this in a later phase, I will update this section to reflect
the actual state, including the phase and PR where it was added.

## Business Rules Verified From My Code

| Rule | Status |
|---|---|
| Order captures product name, price, and image at purchase time instead of a live reference to Product | Confirmed in my code: OrderItem entity stores this data directly rather than through a foreign key |
| No inventory or stock tracking exists yet | Confirmed: no stock field on Product, no stock check in OrderService |
| Basket is not tied to a specific user account | Confirmed: CustomerBasket.Id is a standalone string, not linked to AppUserId |
| Rating requires no proof of purchase | Confirmed: the add rating endpoint has no authorization or purchase check |
| Basket caching uses Redis | Confirmed in infrastructureRegisteration.cs and CustomerBasketRepositry.cs |
| Stripe API key is read from configuration, not hardcoded | Confirmed in PaymentService.cs |
| Stripe webhook does not require user authentication | Expected by design since Stripe calls it externally, but see Security Finding 1 |

## Security Findings From My Own Source Code Audit

1. Critical. The Stripe webhook signing secret is currently hardcoded in
   PaymentsController.cs and committed to a public repository. I need to
   move it to configuration or a secrets manager.

2. High. No Admin role exists yet. The Product and Category Add, Update,
   and Delete endpoints have no authorization attribute, so anyone can
   currently modify the catalog.

3. High. BasketsController has no authorization. Anyone who knows a
   basket's GUID can currently read, modify, or delete it.

4. Medium. The add rating endpoint has no authorization and no check
   that the user actually purchased the product.

5. Low. There is no inventory tracking. This is not a security bug on
   its own, but it is a functional gap for a real e-commerce system, and
   whether it belongs in scope needs a decision.

## Open Decisions

Should I add the Admin role early, during the Product and Category
phase, or defer it to a dedicated security hardening phase?

Is inventory and stock tracking in scope for my production goal, or
explicitly out of scope for this project?

Should the current mandatory registration flow before checkout stay as
is, or should I add guest checkout later?

## Not Yet Verified

I have not yet confirmed the frontend token storage strategy, whether it
uses a cookie or local storage. I will confirm this when I reach the
Angular authentication phase. I have not yet read the internal logic of
EmailService.cs, GenerateToken.cs, or ImageManagementService.cs in
detail. I have not yet checked the actual database migrations against
the entity definitions for consistency.