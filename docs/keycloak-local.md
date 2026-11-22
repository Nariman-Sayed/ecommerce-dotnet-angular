# Running Keycloak locally

Start Keycloak from the repository root:

```bash
cp .env.example .env
docker compose up -d keycloak
```

Compose parses the whole file, so `.env` must exist even when you start only
Keycloak.

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

`Authentication:MetadataAddress` is optional. When it is set, the API keeps the
public `Authority` for token issuers and browser redirects but fetches the
discovery document from that address. This is how the API container reaches
Keycloak as `http://keycloak:8080` while tokens are still issued for
`http://localhost:8080/realms/ecom`. Leave it unset when the API can reach the
authority directly.

The full container stack, including the API, is described in
[docker-local.md](docker-local.md).

## Getting a token for Swagger

1. Start the API in Development so Swagger is enabled.
2. Open `http://localhost:5262/swagger`.
3. Click **Authorize**.
4. The authorization code flow uses the public `ecom-frontend` client with PKCE.
   Log in as one of the development users above.
5. Close the dialog. Swagger UI sends the access token with requests to
   protected endpoints.
6. Use **Authorize** > **Logout** to switch to the other user.
