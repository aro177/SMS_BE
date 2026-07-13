namespace Student_Management_System.Dtos.Lessons;

public record LessonResponse(
    long Id,
    long ClassroomId,
    string ClassroomName,
    long? TeacherId,
    string? TeacherName,
    string Title,
    DateTime StartTime,
    DateTime EndTime);

public record LessonFilter(DateTime? From, DateTime? To, long? TeacherId, long? ClassroomId);

public record CreateLessonRequest(long ClassroomId, string? Title, DateTime StartTime, DateTime EndTime);

public record UpdateLessonRequest(long ClassroomId, string Title, DateTime StartTime, DateTime EndTime);
