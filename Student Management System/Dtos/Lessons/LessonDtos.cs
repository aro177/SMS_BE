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
    RepeatStatus RepeatStatus);

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

public record LessonFilter(DateTime? From, DateTime? To, long? TeacherId, long? ClassroomId);

public record CreateLessonRequest(
    long ClassroomId,
    string? Title,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    RepeatStatus RepeatStatus);

public record UpdateLessonRequest(
    long ClassroomId,
    string Title,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    RepeatStatus RepeatStatus);
