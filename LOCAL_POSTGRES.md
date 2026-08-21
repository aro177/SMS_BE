# Supabase PostgreSQL to local Docker PostgreSQL

This setup copies the `public` schema and all application data from Supabase to a local PostgreSQL container. Supabase-managed services (`auth`, `storage`, and `realtime`) remain remote.

## 1. Create local secrets

Create a strong local-only password:

```powershell
New-Item -ItemType Directory -Path secrets -Force
notepad .\secrets\local_postgres_password.txt
```

Put only the password in the file. Then create the Npgsql connection string used by the API:

```powershell
notepad .\secrets\local_db_connection.txt
```

Use the same password:

```text
Host=postgres;Port=5432;Database=sms_local;Username=sms;Password=YOUR_LOCAL_PASSWORD;SSL Mode=Disable
```

Get the **Session pooler** connection URI from Supabase Dashboard > Connect. Session pooler uses IPv4 and port `5432`. Save the complete URI in:

```powershell
notepad .\secrets\supabase_remote_db_uri.txt
```

Example format:

```text
postgresql://postgres.PROJECT_REF:URL_ENCODED_PASSWORD@aws-0-REGION.pooler.supabase.com:5432/postgres?sslmode=require
```

Copy the URI from Supabase instead of guessing the region or username. If manually inserting a password into a URI, URL-encode reserved characters.

## 2. Start PostgreSQL local

Run from the folder containing both Compose files:

```powershell
docker compose --env-file .env -f docker-compose.yml -f docker-compose.local-db.yml up -d postgres
```

The database is available to the host only at `127.0.0.1:5433` by default. To use another host port, set `LOCAL_POSTGRES_PORT` in `.env`.

## 3. Pull and restore Supabase data

Warning: this command replaces the current local `public` schema and its data.

```powershell
docker compose --env-file .env -f docker-compose.yml -f docker-compose.local-db.yml --profile db-tools run --rm db-import
```

The dump is retained at `backups/supabase-public.dump`. The `backups/` folder is ignored by Git.

## 4. Run the application against PostgreSQL local

```powershell
docker compose --env-file .env -f docker-compose.yml -f docker-compose.local-db.yml up -d --build --force-recreate
```

Verify which database secret is mounted without displaying its value:

```powershell
docker compose --env-file .env -f docker-compose.yml -f docker-compose.local-db.yml exec studentmanagementsystem sh -c "test -s /run/secrets/db_connection && echo local-db-secret-mounted"
```

## Updating the local copy later

Run the import command again. It creates a fresh dump and atomically replaces the local `public` schema. Stop application writes during the import to avoid losing local-only changes:

```powershell
docker compose --env-file .env -f docker-compose.yml -f docker-compose.local-db.yml stop studentmanagementsystem
docker compose --env-file .env -f docker-compose.yml -f docker-compose.local-db.yml --profile db-tools run --rm db-import
docker compose --env-file .env -f docker-compose.yml -f docker-compose.local-db.yml up -d studentmanagementsystem
```

## Returning to Supabase PostgreSQL

Start only the base Compose file. It mounts `secrets/db_connection.txt` again:

```powershell
docker compose --env-file .env -f docker-compose.yml up -d --force-recreate studentmanagementsystem
```

Do not run the local override when you want the API to use the Supabase database directly.
