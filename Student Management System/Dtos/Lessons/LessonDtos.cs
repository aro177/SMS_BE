using Student_Management_System.Models.Enum;

namespace Student_Management_System.Dtos.Lessons;

public record LessonResponse(
    long Id,
    long ClassroomId,
    string ClassroomName,
    long? TeacherId,
    string? TeacherName,
    string Title,
    DateTime StartTime,
    DateTime EndTime,
    string Code,
    bool TakeAttendanceStatus,
    RepeatStatus RepeatStatus,
    Guid? SeriesId);

public record LessonAttendanceResponse(
    long Id,
    long LessonId,
    long StudentId,
    string StudentName,
    string Status,
    string? Note,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record TakeAttendanceStatusResponse(long LessonId, bool TakeAttendanceStatus);

public record BulkTakeAttendanceStatusResponse(
    DateOnly Date,
    bool TakeAttendanceStatus,
    int UpdatedLessons);

public record LessonFilter(DateTime? From, DateTime? To, long? TeacherId, long? ClassroomId);

public enum RecurrenceEndType
{
    AfterWeeks,
    OnDate,
    AfterOccurrences
}

public record CustomLessonRecurrenceRequest(
    int RepeatWeeks,
    IReadOnlyList<DayOfWeek> Weekdays,
    RecurrenceEndType EndType,
    DateOnly? EndDate,
    int? OccurrenceCount);

public record CreateLessonsResponse(
    IReadOnlyList<LessonResponse> Lessons,
    int CreatedCount);

public enum LessonDeleteScope
{
    ThisEvent,
    ThisAndFollowing,
    EntireSeries
}

public enum LessonDeleteOutcome
{
    Deleted,
    NotFound,
    AttendanceHistoryExists,
    SeriesUnavailable,
    InvalidScope
}

public record LessonDeleteResult(
    LessonDeleteOutcome Outcome,
    IReadOnlyList<long> DeletedLessonIds);

public record DeleteLessonsResponse(
    IReadOnlyList<long> DeletedLessonIds,
    int DeletedCount);

public record CreateLessonRequest(
    long ClassroomId,
    string? Title,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    RepeatStatus RepeatStatus,
    CustomLessonRecurrenceRequest? Recurrence);

public record UpdateLessonRequest(
    long ClassroomId,
    string Title,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    RepeatStatus RepeatStatus);
