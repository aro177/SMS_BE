ALTER TABLE lessons
ADD COLUMN IF NOT EXISTS take_attendance_status boolean NOT NULL DEFAULT false;
