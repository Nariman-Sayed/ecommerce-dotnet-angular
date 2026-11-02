# Running Keycloak locally

Start Keycloak from the repository root:

```bash
docker compose up -d keycloak
```

Keycloak listens on `http://localhost:8080`, and the `ecom` realm is imported on
first start. The Compose file publishes the port on `127.0.0.1`, so Keycloak is
reachable from this machine only. The development users are:

- `admin` / `admin-password` (admin role)
- `customer` / `customer-password` (customer role)

These credentials are fake and only exist in the local realm export.

## API authentication configuration

For local development, the API reads `Authentication:Authority` and
`Authentication:Audience` from
`backend/Ecom/Ecom.API/appsettings.Development.json`. The authority points at the
local realm:

```text
http://localhost:8080/realms/ecom
```

Outside Development, the base `appsettings.json` does not contain these values.
Provide them through configuration or the `Authentication__Authority` and
`Authentication__Audience` environment variables. Startup fails with a clear
message when either value is missing.

## Getting a token for Swagger

1. Start the API in Development so Swagger is enabled.
2. Open `http://localhost:5262/swagger`.
3. Click **Authorize**.
4. The authorization code flow uses the public `ecom-frontend` client with PKCE.
   Log in as one of the development users above.
5. Close the dialog. Swagger UI sends the access token with requests to
   protected endpoints.
6. Use **Authorize** > **Logout** to switch to the other user.
