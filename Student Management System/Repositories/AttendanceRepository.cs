using Microsoft.EntityFrameworkCore;
using Student_Management_System.Dtos.Attendances;
using Student_Management_System.Models;
using Student_Management_System.Models.Enum;
using Student_Management_System.Repositories.Interfaces;

namespace Student_Management_System.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly AppDbContext _context;

    public AttendanceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AttendanceStudentResponse>> GetLessonRosterAsync(long lessonId)
    {
        var lesson = await _context.Lessons
            .AsNoTracking()
            .Where(item => !item.IsDeleted && item.Id == lessonId)
            .Select(item => new { item.ClassroomId })
            .FirstOrDefaultAsync();

        if (lesson is null)
        {
            return Array.Empty<AttendanceStudentResponse>();
        }

        return await _context.Enrollments
            .AsNoTracking()
            .Where(enrollment =>
                !enrollment.IsDeleted &&
                enrollment.Status.ToString().ToLower() == EnrollmentStatus.ACTIVE.ToString().ToLower() &&
                enrollment.ClassroomId == lesson.ClassroomId)
            .OrderBy(enrollment => enrollment.Student.Fullname)
            .Select(enrollment => new AttendanceStudentResponse(
                enrollment.StudentId,
                enrollment.Student.Fullname,
                enrollment.Student.Attendances
                    .Where(attendance => !attendance.IsDeleted && attendance.LessonId == lessonId)
                    .Select(attendance => (long?)attendance.Id)
                    .FirstOrDefault(),
                enrollment.Student.Attendances
                    .Where(attendance => !attendance.IsDeleted && attendance.LessonId == lessonId)
                    .Select(attendance => attendance.Status.ToString().ToUpper())
                    .FirstOrDefault(),
                enrollment.Student.Attendances
                    .Where(attendance => !attendance.IsDeleted && attendance.LessonId == lessonId)
                    .Select(attendance => attendance.Note)
                    .FirstOrDefault()))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AttendanceHistoryResponse>> GetStudentHistoryAsync(long studentId)
    {
        return await _context.Attendances
            .AsNoTracking()
            .Where(attendance => !attendance.IsDeleted && attendance.StudentId == studentId)
            .OrderByDescending(attendance => attendance.Lesson.StartTime)
            .Select(attendance => new AttendanceHistoryResponse(
                attendance.Id,
                attendance.LessonId,
                attendance.Lesson.Title,
                attendance.Lesson.Classroom.Name,
                attendance.Lesson.StartTime,
                attendance.Lesson.EndTime,
                attendance.Status.ToString().ToUpper(),
                attendance.Note))
            .ToListAsync();
    }

    public Task<Attendance?> GetByLessonAndStudentAsync(long lessonId, long studentId)
    {
        return _context.Attendances.FirstOrDefaultAsync(attendance =>
            !attendance.IsDeleted &&
            attendance.LessonId == lessonId &&
            attendance.StudentId == studentId);
    }

    public void Add(Attendance attendance)
    {
        _context.Attendances.Add(attendance);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
