using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Classrooms;
using Student_Management_System.Models;

namespace Student_Management_System.Repositories.Interfaces;

public interface IClassroomRepository
{
    Task<PagedResult<ClassroomResponse>> GetPagedAsync(PaginationQuery pagination);
    Task<Classroom?> GetActiveByIdAsync(long id);
    void Add(Classroom classroom);
    Task SaveChangesAsync();
}
