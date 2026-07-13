using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Classrooms;

namespace Student_Management_System.Services.Interfaces;

public interface IClassroomService
{
    Task<PagedResult<ClassroomResponse>> GetPagedAsync(PaginationQuery pagination);
    Task<ClassroomResponse> CreateAsync(CreateClassroomRequest request);
    Task<bool> UpdateAsync(long id, UpdateClassroomRequest request);
    Task<bool> DeleteAsync(long id);
}
