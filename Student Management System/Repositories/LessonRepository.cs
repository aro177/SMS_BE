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

        var projected = query
            .OrderBy(lesson => lesson.StartTime)
            .Select(lesson => new LessonResponse(
                lesson.Id,
                lesson.ClassroomId,
                lesson.Classroom.Name,
                lesson.Classroom.TeacherId,
                lesson.Classroom.Teacher == null ? null : lesson.Classroom.Teacher.Fullname,
                lesson.Title,
                lesson.StartTime,
                lesson.EndTime));

        var total = await projected.CountAsync();
        var items = await projected.Skip(pagination.Skip).Take(pagination.PageSize).ToListAsync();

        return new PagedResult<LessonResponse>(items, pagination.Page, pagination.PageSize, total);
    }

    public Task<Lesson?> GetActiveByIdAsync(long id)
    {
        return _context.Lessons.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
    }

    public void Add(Lesson lesson)
    {
        _context.Lessons.Add(lesson);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
