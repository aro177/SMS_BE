ALTER TABLE public.lessons
ADD COLUMN IF NOT EXISTS series_id uuid;

WITH fixed_series AS (
    SELECT
        classroom_id,
        title,
        created_at,
        gen_random_uuid() AS series_id
    FROM public.lessons
    WHERE repeat_status = 'FIXED'::repeat_status
      AND series_id IS NULL
    GROUP BY classroom_id, title, created_at
)
UPDATE public.lessons AS lesson
SET series_id = fixed_series.series_id
FROM fixed_series
WHERE lesson.classroom_id = fixed_series.classroom_id
  AND lesson.title = fixed_series.title
  AND lesson.created_at = fixed_series.created_at
  AND lesson.repeat_status = 'FIXED'::repeat_status
  AND lesson.series_id IS NULL;

CREATE INDEX IF NOT EXISTS idx_lessons_series_id_active
ON public.lessons (series_id)
WHERE is_deleted = false AND series_id IS NOT NULL;
