using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Lessons;

namespace Student_Management_System.Services.Interfaces;

public interface ILessonService
{
    Task<PagedResult<LessonResponse>> GetPagedAsync(LessonFilter filter, PaginationQuery pagination);
    Task<IReadOnlyList<LessonResponse>?> GetTodayForCurrentTeacherAsync(DateOnly? date = null);
    Task<LessonResponse?> CreateAsync(CreateLessonRequest request);
    Task<bool> UpdateAsync(long id, UpdateLessonRequest request);
    Task<bool> DeleteAsync(long id);
}
