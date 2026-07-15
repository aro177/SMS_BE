using Student_Management_System.Models.Enum;

namespace Student_Management_System.Dtos.Attendances;

public record AttendanceStudentResponse(
    long StudentId,
    string StudentName,
    long? AttendanceId,
    AttendanceStatus? Status,
    string? Note);

public record AttendanceHistoryResponse(
    long Id,
    long LessonId,
    string LessonTitle,
    string ClassroomName,
    DateTime StartTime,
    DateTime EndTime,
    AttendanceStatus Status,
    string? Note);

public record AttendanceMarkRequest(
    long StudentId,
    AttendanceStatus Status,
    string? Note);

public record BulkAttendanceRequest(IReadOnlyList<AttendanceMarkRequest> Items);
