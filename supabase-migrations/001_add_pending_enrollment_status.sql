ALTER TYPE enrollment_status ADD VALUE IF NOT EXISTS 'PENDING';

ALTER TABLE classrooms
ADD COLUMN IF NOT EXISTS age_group varchar(50),
ADD COLUMN IF NOT EXISTS description text,
ADD COLUMN IF NOT EXISTS capacity integer NOT NULL DEFAULT 20;

ALTER TABLE teachers
ADD COLUMN IF NOT EXISTS auth_user_id uuid;

CREATE UNIQUE INDEX IF NOT EXISTS uq_teachers_auth_user_id
ON teachers(auth_user_id)
WHERE auth_user_id IS NOT NULL;
