using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Teachers;
using Student_Management_System.Models;

namespace Student_Management_System.Repositories.Interfaces;

public interface ITeacherRepository
{
    Task<PagedResult<TeacherResponse>> GetPagedAsync(PaginationQuery pagination);
    Task<Teacher?> GetActiveByIdAsync(long id);
    Task<Teacher?> GetActiveByAuthUserIdAsync(Guid authUserId);
    void Add(Teacher teacher);
    Task SaveChangesAsync();
}
