using Student_Management_System.Common.DateTimes;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Configs.HttpContext;
using Student_Management_System.Dtos.Lessons;
using Student_Management_System.Models;
using Student_Management_System.Repositories.Interfaces;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Services;

public class LessonService : ILessonService
{
    private readonly IClassroomRepository _classrooms;
    private readonly ICurrentUserService _currentUser;
    private readonly ILessonRepository _lessons;
    private readonly ITeacherRepository _teachers;

    public LessonService(
        IClassroomRepository classrooms,
        ICurrentUserService currentUser,
        ILessonRepository lessons,
        ITeacherRepository teachers)
    {
        _classrooms = classrooms;
        _currentUser = currentUser;
        _lessons = lessons;
        _teachers = teachers;
    }

    public Task<PagedResult<LessonResponse>> GetPagedAsync(LessonFilter filter, PaginationQuery pagination)
    {
        var normalizedFilter = new LessonFilter(
            DateTimeUtc.Normalize(filter.From),
            DateTimeUtc.Normalize(filter.To),
            filter.TeacherId,
            filter.ClassroomId);

        return _lessons.GetPagedAsync(normalizedFilter, pagination);
    }

    public async Task<IReadOnlyList<LessonResponse>?> GetTodayForCurrentTeacherAsync(DateOnly? date = null)
    {
        var userId = _currentUser.User?.UserId;
        if (userId is null)
        {
            return null;
        }

        var teacher = await _teachers.GetActiveByAuthUserIdAsync(userId.Value);
        if (teacher is null)
        {
            return null;
        }

        var targetDate = date ?? DateTimeUtc.TodayInVietnam();
        var start = DateTimeUtc.FromVietnamLocal(targetDate, TimeOnly.MinValue);
        var end = DateTimeUtc.FromVietnamLocal(targetDate, TimeOnly.MaxValue);
        var lessons = await _lessons.GetPagedAsync(new LessonFilter(start, end, teacher.Id, null), new PaginationQuery { Page = 1, PageSize = 100 });

        return lessons.Items;
    }

    public async Task<LessonResponse?> CreateAsync(CreateLessonRequest request)
    {
        var startTime = DateTimeUtc.Normalize(request.StartTime);
        var endTime = DateTimeUtc.Normalize(request.EndTime);
        var classroom = await _classrooms.GetActiveByIdAsync(request.ClassroomId);
        if (classroom is null || endTime <= startTime)
        {
            return null;
        }

        var lesson = new Lesson
        {
            ClassroomId = request.ClassroomId,
            Title = string.IsNullOrWhiteSpace(request.Title) ? classroom.Name : request.Title.Trim(),
            StartTime = startTime,
            EndTime = endTime,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _lessons.Add(lesson);
        await _lessons.SaveChangesAsync();

        return new LessonResponse(
            lesson.Id,
            lesson.ClassroomId,
            classroom.Name,
            classroom.TeacherId,
            classroom.Teacher?.Fullname,
            lesson.Title,
            lesson.StartTime,
            lesson.EndTime);
    }

    public async Task<bool> UpdateAsync(long id, UpdateLessonRequest request)
    {
        var startTime = DateTimeUtc.Normalize(request.StartTime);
        var endTime = DateTimeUtc.Normalize(request.EndTime);
        var lesson = await _lessons.GetActiveByIdAsync(id);
        var classroom = await _classrooms.GetActiveByIdAsync(request.ClassroomId);
        if (lesson is null || classroom is null || endTime <= startTime)
        {
            return false;
        }

        lesson.ClassroomId = request.ClassroomId;
        lesson.Title = request.Title.Trim();
        lesson.StartTime = startTime;
        lesson.EndTime = endTime;
        lesson.UpdatedAt = DateTime.UtcNow;

        await _lessons.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var lesson = await _lessons.GetActiveByIdAsync(id);
        if (lesson is null)
        {
            return false;
        }

        lesson.IsDeleted = true;
        lesson.UpdatedAt = DateTime.UtcNow;

        await _lessons.SaveChangesAsync();
        return true;
    }
}
