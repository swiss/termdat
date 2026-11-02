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
dotnet user-secrets init --project src/Bk.Termdat.Private.Api
dotnet user-secrets set "pgsql:Username" "postgres" --project src/Bk.Termdat.Private.Api
dotnet user-secrets set "pgsql:Password" "postgres" --project src/Bk.Termdat.Private.Api
dotnet user-secrets set "rabbitmq:default_user" "guest" --project src/Bk.Termdat.Private.Api
dotnet user-secrets set "rabbitmq:default_pass" "guest" --project src/Bk.Termdat.Private.Api
dotnet user-secrets set "DocumentService:ClientId" "your_document_service_client_id" --project src/Bk.Termdat.Private.Api
dotnet user-secrets set "DocumentService:ClientSecret" "your_document_service_secret" --project src/Bk.Termdat.Private.Api
dotnet user-secrets set "Frontend:FroalaKey" "your_froala_key" --project src/Bk.Termdat.Private.Api
dotnet user-secrets set "AutoMapper:LicenseKey" "your_automapper_license_key" --project src/Bk.Termdat.Private.Api
```

- `DocumentService` credentials are placeholders: the Document Service is an internal system which is only required for specific features and is not reachable locally.
- `Frontend:FroalaKey` is the license key for the Froala rich text editor. Without a valid key the editor shows a watermark.

### Bk.Termdat.Search.Api

```
dotnet user-secrets init --project src/Bk.Termdat.Search.Api
dotnet user-secrets set "pgsql:Username" "postgres" --project src/Bk.Termdat.Search.Api
dotnet user-secrets set "pgsql:Password" "postgres" --project src/Bk.Termdat.Search.Api
dotnet user-secrets set "rabbitmq:default_user" "guest" --project src/Bk.Termdat.Search.Api
dotnet user-secrets set "rabbitmq:default_pass" "guest" --project src/Bk.Termdat.Search.Api
```

### Bk.Termdat.Public.Api

```
dotnet user-secrets init --project src/Bk.Termdat.Public.Api
dotnet user-secrets set "pgsql:Username" "postgres" --project src/Bk.Termdat.Public.Api
dotnet user-secrets set "pgsql:Password" "postgres" --project src/Bk.Termdat.Public.Api
dotnet user-secrets set "AutoMapper:LicenseKey" "your_automapper_license_key" --project src/Bk.Termdat.Public.Api
```

### Bk.Termdat.Gateway

```
dotnet user-secrets init --project src/Bk.Termdat.Gateway
dotnet user-secrets set "OpenIdConnect:ClientSecret" "f8056df3-876b-4b9a-af5a-f08df7909902" --project src/Bk.Termdat.Gateway
```

The client secret matches the `bk-termdat` client defined in the KeyCloak realm export file (see above).

### Bk.Termdat.LindasExport.Api

```
dotnet user-secrets init --project src/Bk.Termdat.LindasExport.Api
dotnet user-secrets set "pgsql:Username" "postgres" --project src/Bk.Termdat.LindasExport.Api
dotnet user-secrets set "pgsql:Password" "postgres" --project src/Bk.Termdat.LindasExport.Api
dotnet user-secrets set "SparqlTargets:Targets:LindasNext:Username" "your_lindas_username" --project src/Bk.Termdat.LindasExport.Api
dotnet user-secrets set "SparqlTargets:Targets:LindasNext:Password" "your_lindas_password" --project src/Bk.Termdat.LindasExport.Api
dotnet user-secrets set "SparqlTargets:Targets:LindasNext:GraphStoreProtocolEndPoint" "your_lindas_graph_store_end_point" --project src/Bk.Termdat.LindasExport.Api
dotnet user-secrets set "SparqlTargets:Targets:LindasNext:HealthEndPoint" "your_lindas_health_end_point" --project src/Bk.Termdat.LindasExport.Api
dotnet user-secrets set "SparqlTargets:Targets:LindasNext:Proxy:Address" "your_proxy_address" --project src/Bk.Termdat.LindasExport.Api
```

- The `SparqlTargets` credentials, endpoints and proxy address point to internal systems and will not resolve outside the BIT network. Without them the RDF export to LINDAS will not work locally.

Note: some values in the `appsettings.Development.json` files (e.g. Document Service, MyAccount) point to internal systems and will not resolve outside the BIT network. Features relying on them will not work locally.

## Entity Framework
To manipulate and initially create the local database you can use the EF Core tools.<br>
```dotnet tool install --global dotnet-ef```

Create initial database or update to the latest migration.<br>
```dotnet ef database update -s src/Bk.Termdat.Private.Api -p src/Bk.Termdat.Infrastructure.DataSource```

Note: in Development the Private API automatically migrates the database on startup (`Migration:IsAutoMigration=true`), so the manual step above is usually not necessary. On an empty database, both ways stop at the same migration, see [Initialize an empty database](#initialize-an-empty-database).

Generate new EF migrations using the following command.<br>
```dotnet ef migrations add migration_name_here -s src/Bk.Termdat.Private.Api -p src/Bk.Termdat.Infrastructure.DataSource```

## Initialize an empty database
The migration `AddNoteSubCodesWithTranslations` expects the languages to exist already, but no migration creates them. On an empty database the Private API therefore stops at startup with a foreign key error on `note_sub_code_translations`. Migrations before this one are applied.

1. Start the Private API once and let it stop with this error: ```dotnet run --project src/Bk.Termdat.Private.Api```
2. Save the script below as `seed-languages.sql` and add the languages: ```docker exec -i termdat-postgres psql -U postgres -d termdat -v ON_ERROR_STOP=1 < seed-languages.sql```
3. Start the Private API again. It applies the remaining migrations.

```
-- Local development seed: must run after the migrations up to 20251006092659_AddExternalReferences.
-- Migration 20260619142616_AddNoteSubCodesWithTranslations assumes these language ids exist,
-- so it fails on an empty database. The ids and their languages are derived from that migration.
BEGIN;

INSERT INTO data.languages (id, iso_code, is_ui_language, is_system_language, sort_index, created, created_by, modified, modified_by)
VALUES
    (2, 'DE', true, true, 1, now(), 'seed', now(), 'seed'),
    (6, 'FR', true, true, 2, now(), 'seed', now(), 'seed'),
    (7, 'IT', true, true, 3, now(), 'seed', now(), 'seed'),
    (8, 'RM', true, true, 4, now(), 'seed', now(), 'seed'),
    (3, 'EN', true, true, 5, now(), 'seed', now(), 'seed')
ON CONFLICT (id) DO NOTHING;

SELECT setval(pg_get_serial_sequence('data.languages', 'id'), GREATEST((SELECT max(id) FROM data.languages), 8));

INSERT INTO data.language_translations (language_id, iso_code, name)
SELECT v.language_id, v.iso_code, v.name
FROM (VALUES
    (2, 'DE', 'Deutsch'),      (2, 'FR', 'Allemand'),  (2, 'IT', 'Tedesco'),  (2, 'RM', 'Tudestg'),   (2, 'EN', 'German'),
    (6, 'DE', 'Französisch'),  (6, 'FR', 'Français'),  (6, 'IT', 'Francese'), (6, 'RM', 'Franzos'),   (6, 'EN', 'French'),
    (7, 'DE', 'Italienisch'),  (7, 'FR', 'Italien'),   (7, 'IT', 'Italiano'), (7, 'RM', 'Talian'),    (7, 'EN', 'Italian'),
    (8, 'DE', 'Rätoromanisch'),(8, 'FR', 'Romanche'),  (8, 'IT', 'Romancio'), (8, 'RM', 'Rumantsch'), (8, 'EN', 'Romansh'),
    (3, 'DE', 'Englisch'),     (3, 'FR', 'Anglais'),   (3, 'IT', 'Inglese'),  (3, 'RM', 'Englais'),   (3, 'EN', 'English')
) AS v(language_id, iso_code, name)
WHERE NOT EXISTS (
    SELECT 1 FROM data.language_translations t
    WHERE t.language_id = v.language_id AND t.iso_code = v.iso_code
);

COMMIT;
```

## Run the backend services
The solution consists of several .NET services. The ports are configured in the `launchSettings.json` of each project and the `ASPNETCORE_ENVIRONMENT` is already set to `Development`. Required for the UI are the Private API, the Search API and the Gateway.

Run each service in its own terminal. Start the Private API first and wait until it is listening: it migrates the database, and the Search API stops if the tables do not exist yet.

```
dotnet run --project src/Bk.Termdat.Private.Api      # http://localhost:22265
dotnet run --project src/Bk.Termdat.Search.Api       # http://localhost:22266
dotnet run --project src/Bk.Termdat.Gateway          # http://localhost:22271
```

Optional services (e.g. for API development):

```
dotnet run --project src/Bk.Termdat.Public.Api        # http://localhost:22267
dotnet run --project src/Bk.Termdat.LindasExport.Api  # http://localhost:22270
```

Swagger UI:
- Private API: http://localhost:22265/api/swagger
- Search API: http://localhost:22266/api/swagger
- Public API: http://localhost:22267/swagger
- Lindas Export API: http://localhost:22270/api/swagger

## Angular Frontend
Navigate to ```src/Bk.Termdat.UI```

Install dependencies with ```npm ci```

Start the angular app with ```npm run start```

The dev server runs on http://localhost:4445 and proxies `/api` calls to the Private API (http://localhost:22265).
Open the application via the **Gateway** at http://localhost:22271 — the login (`/auth/*`) is provided by the Gateway, so running the Angular dev server alone is not sufficient.

## Additional notes
- MailHog (http://localhost:8044) catches all emails sent by the application (e.g. feedback and file import notifications).
- Matomo analytics is disabled in Development.
- The PostgreSQL data is available via pgAdmin at http://localhost:9092 (optional service).
