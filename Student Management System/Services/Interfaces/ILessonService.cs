using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Lessons;

namespace Student_Management_System.Services.Interfaces;

public interface ILessonService
{
    Task<PagedResult<LessonResponse>> GetPagedAsync(LessonFilter filter, PaginationQuery pagination);
    Task<IReadOnlyList<LessonAttendanceResponse>?> GetAttendancesAsync(long lessonId);
    Task<IReadOnlyList<LessonResponse>?> GetTodayForCurrentTeacherAsync(DateOnly? date = null);
    Task<IReadOnlyList<LessonResponse>> GetTodayAsync(DateOnly? date = null);
    Task<TakeAttendanceStatusResponse?> ToggleTakeAttendanceStatusAsync(long lessonId);
    Task<BulkTakeAttendanceStatusResponse> ToggleTodayTakeAttendanceStatusAsync(DateOnly? date = null);
    Task<CreateLessonsResponse?> CreateAsync(CreateLessonRequest request);
    Task<bool> UpdateAsync(long id, UpdateLessonRequest request);
    Task<bool> DeleteAsync(long id);
}
