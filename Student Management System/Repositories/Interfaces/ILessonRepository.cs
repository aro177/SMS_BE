using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Lessons;
using Student_Management_System.Models;

namespace Student_Management_System.Repositories.Interfaces;

public interface ILessonRepository
{
    Task<PagedResult<LessonResponse>> GetPagedAsync(LessonFilter filter, PaginationQuery pagination);
    Task<IReadOnlyList<LessonAttendanceResponse>> GetAttendancesAsync(long lessonId);
    Task<Lesson?> GetActiveByIdAsync(long id);
    void Add(Lesson lesson);
    Task SaveChangesAsync();
}
