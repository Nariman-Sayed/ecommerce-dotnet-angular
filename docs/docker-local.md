# Running the stack with Docker Compose

The Compose file runs SQL Server, Keycloak, and the API. Start from the
repository root.

## First start

```bash
cp .env.example .env
```

Edit `.env` and replace the example value with your own local-only password.
The Compose file requires `MSSQL_SA_PASSWORD`; it has no default. The value is
used for the SQL Server `sa` account and for the API connection string, so keep
it free of semicolons. `.env` is git-ignored.

```bash
docker compose up -d --build
docker compose ps
```

The API waits for the SQL Server health check. Its first start also applies the
EF Core migrations because `Database__MigrateOnStartup` is enabled for this local
stack, then seeds the demo catalogue.

## URLs and ports

| Service | Address | Notes |
| --- | --- | --- |
| API | `http://localhost:5262` | `GET http://localhost:5262/api/products` |
| Swagger | `http://localhost:5262/swagger` | Available because the API runs with `ASPNETCORE_ENVIRONMENT=Development` |
| Keycloak | `http://localhost:8080` | Realm `ecom`; `admin` / `admin-password`, `customer` / `customer-password` |
| SQL Server | `127.0.0.1:14330` | User `sa`, password from `.env` |

Only loopback addresses are published. The API reaches SQL Server as
`sqlserver:1433` and Keycloak as `keycloak:8080` on the Compose network.

Keycloak-only development still works:

```bash
docker compose up -d keycloak
```

Compose reads the whole file, so `.env` must exist even when you start only
Keycloak.

## Swagger login

1. Open `http://localhost:5262/swagger`.
2. Click **Authorize** and log in as `admin` / `admin-password` to create or
   change data.
3. Use **Authorize** > **Logout** and log in as `customer` /
   `customer-password` to check the read-only role.

## Uploads

Product images are stored in the `uploads` volume at `/app/wwwroot` in the API
container. The API runs as the non-root `app` user and owns that directory.

## Logs and shutdown

```bash
docker compose logs -f api
docker compose down
```

`docker compose down` keeps both named volumes: SQL Server data and uploaded
images. Stop and delete everything, including the database and images, with:

```bash
docker compose down -v
```

## Demo-only settings

This stack is for local development, not deployment:

- The API runs as `Development`, which enables Swagger and the development-only
  catalogue seeding.
- Startup migrations are enabled. Several API instances would race to apply the
  same migration, so the flag stays off by default outside this stack.
- Keycloak uses `start-dev`, the bootstrap `admin` / `admin` account, and the
  checked-in realm export with fake users.
- The SQL Server `sa` password comes from your local `.env`; there are no
  production secret-management settings here.
