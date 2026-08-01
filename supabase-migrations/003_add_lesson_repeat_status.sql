DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_type
        WHERE typname = 'repeat_status'
    ) THEN
        CREATE TYPE repeat_status AS ENUM ('FIXED', 'TEMPORARY');
    END IF;
END
$$;

ALTER TABLE lessons
ADD COLUMN IF NOT EXISTS repeat_status repeat_status NOT NULL DEFAULT 'TEMPORARY';
