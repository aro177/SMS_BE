using Student_Management_System.Dtos.Attendances;

namespace Student_Management_System.Services.Interfaces;

public interface IAttendanceService
{
    Task<IReadOnlyList<AttendanceStudentResponse>?> GetLessonRosterAsync(long lessonId);
    Task<IReadOnlyList<AttendanceHistoryResponse>> GetStudentHistoryAsync(long studentId);
    Task<bool> MarkLessonAsync(long lessonId, BulkAttendanceRequest request);
}
