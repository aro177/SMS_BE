using Student_Management_System.Dtos.Attendances;
using Student_Management_System.Models;
using Student_Management_System.Repositories.Interfaces;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendances;
    private readonly ILessonRepository _lessons;

    public AttendanceService(IAttendanceRepository attendances, ILessonRepository lessons)
    {
        _attendances = attendances;
        _lessons = lessons;
    }

    public async Task<IReadOnlyList<AttendanceStudentResponse>?> GetLessonRosterAsync(long lessonId)
    {
        var lesson = await _lessons.GetActiveByIdAsync(lessonId);
        if (lesson is null)
        {
            return null;
        }

        return await _attendances.GetLessonRosterAsync(lessonId);
    }

    public Task<IReadOnlyList<AttendanceHistoryResponse>> GetStudentHistoryAsync(long studentId)
    {
        return _attendances.GetStudentHistoryAsync(studentId);
    }

    public async Task<bool> MarkLessonAsync(long lessonId, BulkAttendanceRequest request)
    {
        var lesson = await _lessons.GetActiveByIdAsync(lessonId);
        if (lesson is null)
        {
            return false;
        }

        foreach (var item in request.Items)
        {
            var attendance = await _attendances.GetByLessonAndStudentAsync(lessonId, item.StudentId);
            if (attendance is null)
            {
                attendance = new Attendance
                {
                    LessonId = lessonId,
                    StudentId = item.StudentId,
                    Status = item.Status,
                    Note = string.IsNullOrWhiteSpace(item.Note) ? null : item.Note.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _attendances.Add(attendance);
            }
            else
            {
                attendance.Status = item.Status;
                attendance.Note = string.IsNullOrWhiteSpace(item.Note) ? null : item.Note.Trim();
                attendance.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _attendances.SaveChangesAsync();
        return true;
    }
}
