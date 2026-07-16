using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Teachers;

namespace Student_Management_System.Services.Interfaces;

public interface ITeacherService
{
    Task<PagedResult<TeacherResponse>> GetPagedAsync(PaginationQuery pagination);
    Task<TeacherResponse?> GetCurrentTeacherAsync();
    Task<TeacherResponse> CreateAsync(CreateTeacherRequest request);
    Task<bool> UpdateAsync(long id, UpdateTeacherRequest request);
    Task<bool> DeleteAsync(long id);
}
