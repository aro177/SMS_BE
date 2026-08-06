# Docker secrets

Create these four files locally before starting the container:

- `db_connection.txt`: the value of `ConnectionStrings:DefaultConnection` from User Secrets.
- `supabase_key.txt`: the value of `SUPABASE_KEY` used to initialize the Supabase SDK.
- `supabase_api_secret_key.txt`: the server-side/service-role key used by `Supabase:ApiSecretKey` for Admin Auth requests.
- `turnstile_secret_key.txt`: the private Cloudflare Turnstile secret key used only by the backend for Siteverify requests.

Only this README is tracked. Every other file in this directory is ignored by Git and excluded from the Docker build context.

Do not store the Supabase key, Turnstile secret key, or database connection string in `.env`, `appsettings*.json`, the Dockerfile, Docker build arguments, or Git. The Turnstile site key is public and belongs in `.env` as `TURNSTILE_SITE_KEY`.
