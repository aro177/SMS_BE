using Microsoft.EntityFrameworkCore;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Lessons;
using Student_Management_System.Models;
using Student_Management_System.Repositories.Interfaces;

namespace Student_Management_System.Repositories;

public class LessonRepository : ILessonRepository
{
    private readonly AppDbContext _context;

    public LessonRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<LessonResponse>> GetPagedAsync(LessonFilter filter, PaginationQuery pagination)
    {
        var query = _context.Lessons
            .AsNoTracking()
            .Where(lesson => !lesson.IsDeleted);

        if (filter.From is not null)
        {
            query = query.Where(lesson => lesson.StartTime >= filter.From);
        }

        if (filter.To is not null)
        {
            query = query.Where(lesson => lesson.EndTime <= filter.To);
        }

        if (filter.TeacherId is not null)
        {
            query = query.Where(lesson => lesson.Classroom.TeacherId == filter.TeacherId);
        }

        if (filter.ClassroomId is not null)
        {
            query = query.Where(lesson => lesson.ClassroomId == filter.ClassroomId);
        }

        var projected = ProjectResponses(query);

        var total = await projected.CountAsync();
        var items = await projected.Skip(pagination.Skip).Take(pagination.PageSize).ToListAsync();

        return new PagedResult<LessonResponse>(items, pagination.Page, pagination.PageSize, total);
    }

    public async Task<IReadOnlyList<LessonResponse>> GetByStartRangeAsync(
        DateTime start,
        DateTime endExclusive)
    {
        var query = _context.Lessons
            .AsNoTracking()
            .Where(lesson =>
                !lesson.IsDeleted &&
                lesson.StartTime >= start &&
                lesson.StartTime < endExclusive);

        return await ProjectResponses(query).ToListAsync();
    }

    public async Task<IReadOnlyList<Lesson>> GetActiveEntitiesByStartRangeAsync(
        DateTime start,
        DateTime endExclusive)
    {
        return await _context.Lessons
            .Where(lesson =>
                !lesson.IsDeleted &&
                lesson.StartTime >= start &&
                lesson.StartTime < endExclusive)
            .OrderBy(lesson => lesson.StartTime)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<LessonAttendanceResponse>> GetAttendancesAsync(long lessonId)
    {
        return await _context.Attendances
            .AsNoTracking()
            .Where(attendance => !attendance.IsDeleted && attendance.LessonId == lessonId)
            .OrderBy(attendance => attendance.Student.Fullname)
            .Select(attendance => new LessonAttendanceResponse(
                attendance.Id,
                attendance.LessonId,
                attendance.StudentId,
                attendance.Student.Fullname,
                attendance.Status.ToString(),
                attendance.Note,
                attendance.CreatedAt,
                attendance.UpdatedAt))
            .ToListAsync();
    }

    public Task<Lesson?> GetActiveByIdAsync(long id)
    {
        return _context.Lessons.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
    }

    public async Task<IReadOnlyList<Lesson>> GetActiveBySeriesIdAsync(Guid seriesId)
    {
        return await _context.Lessons
            .Where(lesson => !lesson.IsDeleted && lesson.SeriesId == seriesId)
            .OrderBy(lesson => lesson.StartTime)
            .ToListAsync();
    }

    public Task<bool> HasAnyAttendanceHistoryAsync(IReadOnlyCollection<long> lessonIds)
    {
        return _context.Attendances.AnyAsync(attendance => lessonIds.Contains(attendance.LessonId));
    }

    public void Add(Lesson lesson)
    {
        _context.Lessons.Add(lesson);
    }

    public void AddRange(IEnumerable<Lesson> lessons)
    {
        _context.Lessons.AddRange(lessons);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    private static IQueryable<LessonResponse> ProjectResponses(IQueryable<Lesson> query)
    {
        return query
            .OrderBy(lesson => lesson.StartTime)
            .Select(lesson => new LessonResponse(
                lesson.Id,
                lesson.ClassroomId,
                lesson.Classroom.Name,
                lesson.Classroom.TeacherId,
                lesson.Classroom.Teacher == null ? null : lesson.Classroom.Teacher.Fullname,
                lesson.Title,
                lesson.StartTime,
                lesson.EndTime,
                lesson.Code,
                lesson.TakeAttendanceStatus,
                lesson.RepeatStatus,
                lesson.SeriesId));
    }
}
