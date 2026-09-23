# Getting Started
The following steps describe how to set up the project locally.

## Prerequisites
- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0) (pinned via `global.json`)
- [Node.js](https://nodejs.org/) LTS and npm (Angular 21)
- [Docker](https://www.docker.com/)

## Set up Docker containers locally
You can create a docker compose file to run the required services locally.
The following example should be helpful to start. Save it in the repository root (e.g. as `docker-compose.yml`).

```
services:
  termdat-postgres:
    image: postgres:17
    container_name: termdat-postgres
    restart: on-failure
    environment:
      POSTGRES_DB: termdat
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      PGDATA: /var/lib/postgresql/data
    volumes:
      - termdat-db-data:/var/lib/postgresql/data
    ports:
      - "5434:5432"
    networks:
      - termdat-net

  termdat-rabbitmq:
    image: rabbitmq:3-management
    container_name: termdat-rabbitmq
    restart: on-failure
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - termdat-rabbitmq-data:/var/lib/rabbitmq
    networks:
      - termdat-net

  termdat-kcpostgres:
    image: postgres:17
    container_name: termdat-kcpostgres
    restart: on-failure
    volumes:
      - termdat-keycloak-data:/var/lib/postgresql/data
    environment:
      POSTGRES_DB: keycloak
      POSTGRES_USER: administrator
      POSTGRES_PASSWORD: administrator
    networks:
      - termdat-net

  termdat-keycloak:
    image: keycloak/keycloak:24.0.1
    container_name: termdat-keycloak
    restart: on-failure
    volumes:
      - ./etc/termdat-docker/keycloak-realms:/opt/keycloak/data/import
    environment:
      KEYCLOAK_ADMIN: administrator
      KEYCLOAK_ADMIN_PASSWORD: administrator
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://termdat-kcpostgres:5432/keycloak
      KC_DB_USERNAME: administrator
      KC_DB_PASSWORD: administrator
    ports:
      - "9082:8080"
    depends_on:
      - termdat-kcpostgres
    networks:
      - termdat-net
    command: start-dev --import-realm

  termdat-mailhog:
    image: mailhog/mailhog
    container_name: termdat-mailhog
    restart: on-failure
    ports:
      - "1044:1025"
      - "8044:8025"
    networks:
      - termdat-net

  # optional: database administration UI
  termdat-pgadmin:
    image: dpage/pgadmin4:9.3
    container_name: termdat-pgadmin
    restart: on-failure
    environment:
      PGADMIN_DEFAULT_EMAIL: admin@admin.com
      PGADMIN_DEFAULT_PASSWORD: administrator
      PGADMIN_LISTEN_PORT: 80
    ports:
      - "9092:80"
    volumes:
      - termdat-pgadmin-data:/var/lib/pgadmin
    networks:
      - termdat-net

  # optional: analytics (disabled in Development)
  termdat-matomo-db:
    image: mariadb
    container_name: termdat-matomo-db
    command: --max-allowed-packet=64MB
    volumes:
      - termdat-matomo-db-data:/var/lib/mysql
    environment:
      - MYSQL_DATABASE=matomo
      - MYSQL_USER=matomo
      - MYSQL_PASSWORD=administrator
      - MYSQL_ROOT_PASSWORD=administrator
    networks:
      - termdat-net

  termdat-matomo:
    image: matomo
    container_name: termdat-matomo
    depends_on:
      - termdat-matomo-db
    volumes:
      - termdat-matomo-data:/var/www/html
    environment:
      - MATOMO_DATABASE_HOST=termdat-matomo-db
      - MATOMO_DATABASE_ADAPTER=mysql
      - MATOMO_DATABASE_TABLES_PREFIX=matomo_
      - MATOMO_DATABASE_USERNAME=matomo
      - MATOMO_DATABASE_PASSWORD=administrator
      - MATOMO_DATABASE_DBNAME=matomo
    ports:
      - "8045:80"
    networks:
      - termdat-net

networks:
  termdat-net:
    driver: bridge

volumes:
  termdat-db-data:
  termdat-rabbitmq-data:
  termdat-keycloak-data:
    driver: local
  termdat-pgadmin-data:
  termdat-matomo-db-data:
  termdat-matomo-data:
```

Start the containers with `docker compose up -d`.

This docker file contains a reference to a KeyCloak realm export file (`./etc/termdat-docker/keycloak-realms/bk-termdat-realm.json`) which will be imported when KeyCloak is initialized.
The realm is already part of this repository and contains the confidential client `bk-termdat` (used by the Gateway) as well as the following test users. For local development, the username and password are set to the same value:

| Username | Client roles (bk-termdat) |
|---|---|
| admin | ALLOW, Admin |
| writer | ALLOW, Writer |
| superuser | ALLOW, SuperUser |
| superreader | ALLOW, SuperReader |
| allroles | ALLOW, Admin, Writer, SuperUser |
| noallow | Admin, Writer, SuperUser (no ALLOW → treated as unauthorized by the app) |
| allow | ALLOW only |

The app requires the `ALLOW` client role for access; `Admin`, `Writer`, `SuperUser` and `SuperReader` control the available features.

The KeyCloak admin console is available at http://localhost:9082 (username/password: `administrator`/`administrator`).

## Add user secrets

You can add user secrets (see: https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to store the credentials to the local services in a secure way.
The required secrets differ per project. Run the following commands from the repository root.

### Bk.Termdat.Private.Api

```
dotnet user-secrets init --project src\Bk.Termdat.Private.Api
dotnet user-secrets set "pgsql:Username" "postgres" --project src\Bk.Termdat.Private.Api
dotnet user-secrets set "pgsql:Password" "postgres" --project src\Bk.Termdat.Private.Api
dotnet user-secrets set "rabbitmq:default_user" "guest" --project src\Bk.Termdat.Private.Api
dotnet user-secrets set "rabbitmq:default_pass" "guest" --project src\Bk.Termdat.Private.Api
dotnet user-secrets set "DocumentService:ClientId" "your_document_service_client_id" --project src\Bk.Termdat.Private.Api
dotnet user-secrets set "DocumentService:ClientSecret" "your_document_service_secret" --project src\Bk.Termdat.Private.Api
dotnet user-secrets set "Frontend:FroalaKey" "your_froala_key" --project src\Bk.Termdat.Private.Api
dotnet user-secrets set "AutoMapper:LicenseKey" "your_automapper_license_key" --project src\Bk.Termdat.Private.Api
```

- `DocumentService` credentials are placeholders: the Document Service is an internal system which is only required for specific features and is not reachable locally.
- `Frontend:FroalaKey` is the license key for the Froala rich text editor. Without a valid key the editor shows a watermark.

### Bk.Termdat.Search.Api

```
dotnet user-secrets init --project src\Bk.Termdat.Search.Api
dotnet user-secrets set "pgsql:Username" "postgres" --project src\Bk.Termdat.Search.Api
dotnet user-secrets set "pgsql:Password" "postgres" --project src\Bk.Termdat.Search.Api
dotnet user-secrets set "rabbitmq:default_user" "guest" --project src\Bk.Termdat.Search.Api
dotnet user-secrets set "rabbitmq:default_pass" "guest" --project src\Bk.Termdat.Search.Api
```

### Bk.Termdat.Public.Api

```
dotnet user-secrets init --project src\Bk.Termdat.Public.Api
dotnet user-secrets set "pgsql:Username" "postgres" --project src\Bk.Termdat.Public.Api
dotnet user-secrets set "pgsql:Password" "postgres" --project src\Bk.Termdat.Public.Api
dotnet user-secrets set "AutoMapper:LicenseKey" "your_automapper_license_key" --project src\Bk.Termdat.Public.Api
```

### Bk.Termdat.Gateway

```
dotnet user-secrets init --project src\Bk.Termdat.Gateway
dotnet user-secrets set "OpenIdConnect:ClientSecret" "f8056df3-876b-4b9a-af5a-f08df7909902" --project src\Bk.Termdat.Gateway
```

The client secret matches the `bk-termdat` client defined in the KeyCloak realm export file (see above).

### Bk.Termdat.LindasExport.Api

```
dotnet user-secrets init --project src\Bk.Termdat.LindasExport.Api
dotnet user-secrets set "pgsql:Username" "postgres" --project src\Bk.Termdat.LindasExport.Api
dotnet user-secrets set "pgsql:Password" "postgres" --project src\Bk.Termdat.LindasExport.Api
```

Note: some values in the `appsettings.Development.json` files (e.g. Document Service, MyAccount, LINDAS endpoints and proxy) point to internal systems and will not resolve outside the BIT network. Features relying on them will not work locally.

## Entity Framework
To manipulate and initially create the local database you can use the EF Core tools.<br>
```dotnet tool install --global dotnet-ef```

Create initial database or update to the latest migration.<br>
```dotnet ef database update -s src\Bk.Termdat.Private.Api -p src\Bk.Termdat.Infrastructure.DataSource```

Note: in Development the Private API automatically migrates the database on startup (`Migration:IsAutoMigration=true`), so the manual step above is usually not necessary.

Generate new EF migrations using the following command.<br>
```dotnet ef migrations add migration_name_here -s src\Bk.Termdat.Private.Api -p src\Bk.Termdat.Infrastructure.DataSource```

## Run the backend services
The solution consists of several .NET services. The ports are configured in the `launchSettings.json` of each project and the `ASPNETCORE_ENVIRONMENT` is already set to `Development`. Required for the UI are the Private API, the Search API and the Gateway.

```
dotnet run --project src\Bk.Termdat.Private.Api      # http://localhost:22265
dotnet run --project src\Bk.Termdat.Search.Api       # http://localhost:22266
dotnet run --project src\Bk.Termdat.Gateway          # http://localhost:22271
```

Optional services (e.g. for API development):

```
dotnet run --project src\Bk.Termdat.Public.Api        # http://localhost:22267
dotnet run --project src\Bk.Termdat.LindasExport.Api  # http://localhost:22270
```

Swagger UI:
- Private API: http://localhost:22265/api/swagger
- Search API: http://localhost:22266/api/swagger
- Public API: http://localhost:22267/swagger
- Lindas Export API: http://localhost:22270/api/swagger

## Angular Frontend
Navigate to ```src\Bk.Termdat.UI```

Install dependencies with ```npm ci```

Start the angular app with ```npm run start```

The dev server runs on http://localhost:4445 and proxies `/api` calls to the Private API (http://localhost:22265).
Open the application via the **Gateway** at http://localhost:22271 — the login (`/auth/*`) is provided by the Gateway, so running the Angular dev server alone is not sufficient.

Note: the committed `package-lock.json` currently references the internal npm registry (repo.bit.admin.ch). Outside the BIT network, regenerate the lockfile against the public registry first (```npm install --registry https://registry.npmjs.org/```).

## Additional notes
- MailHog (http://localhost:8044) catches all emails sent by the application (e.g. feedback and file import notifications).
- Matomo analytics is disabled in Development.
- The PostgreSQL data is available via pgAdmin at http://localhost:9092 (optional service).
