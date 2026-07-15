using Student_Management_System.Dtos.Attendances;
using Student_Management_System.Models;

namespace Student_Management_System.Repositories.Interfaces;

public interface IAttendanceRepository
{
    Task<IReadOnlyList<AttendanceStudentResponse>> GetLessonRosterAsync(long lessonId);
    Task<IReadOnlyList<AttendanceHistoryResponse>> GetStudentHistoryAsync(long studentId);
    Task<Attendance?> GetByLessonAndStudentAsync(long lessonId, long studentId);
    void Add(Attendance attendance);
    Task SaveChangesAsync();
}
