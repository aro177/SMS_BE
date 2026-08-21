#!/bin/sh
set -eu

remote_db_uri="$(tr -d '\r\n' < /run/secrets/supabase_remote_db_uri)"
local_db_password="$(tr -d '\r\n' < /run/secrets/local_postgres_password)"
backup_path="/backups/supabase-public.dump"

if [ -z "$remote_db_uri" ]; then
    echo "The Supabase remote database URI secret is empty." >&2
    exit 1
fi

if [ -z "$local_db_password" ]; then
    echo "The local PostgreSQL password secret is empty." >&2
    exit 1
fi

echo "Waiting for local PostgreSQL..."
export PGPASSWORD="$local_db_password"
until pg_isready \
    --host="$LOCAL_DB_HOST" \
    --port="$LOCAL_DB_PORT" \
    --username="$LOCAL_DB_USER" \
    --dbname="$LOCAL_DB_NAME"; do
    sleep 2
done

echo "Downloading the public schema and application data from Supabase..."
pg_dump \
    --dbname="$remote_db_uri" \
    --format=custom \
    --file="$backup_path" \
    --schema=public \
    --no-owner \
    --no-privileges \
    --verbose

echo "Replacing the local public schema from the downloaded dump..."
pg_restore \
    --host="$LOCAL_DB_HOST" \
    --port="$LOCAL_DB_PORT" \
    --username="$LOCAL_DB_USER" \
    --dbname="$LOCAL_DB_NAME" \
    --clean \
    --if-exists \
    --no-owner \
    --no-privileges \
    --single-transaction \
    --exit-on-error \
    --verbose \
    "$backup_path"

echo "Import completed. Backup retained at $backup_path."
