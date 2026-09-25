# Ecom.Application

This project is scaffolded ahead of need.

The Application layer will house use-case orchestration services once the
Category and Product controllers (upcoming PRs) move their business logic
out of Ecom.API. It depends only on Ecom.Core, keeping the dependency
direction API -> Application -> Core with Infrastructure sitting alongside,
wired up via DI in Ecom.API.

No services exist yet because PR#1 introduces no controllers, and inventing
placeholder services now would be dead code. The empty project documents the
boundary so the next PRs have a home for orchestration logic.