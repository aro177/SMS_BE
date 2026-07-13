using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Students;

namespace Student_Management_System.Services.Interfaces;

public interface IStudentService
{
    Task<PagedResult<StudentResponse>> GetPagedAsync(PaginationQuery pagination);
    Task<IReadOnlyList<ChildSearchResponse>> SearchChildrenAsync(string parentPhone, DateOnly childDob);
    Task<StudentResponse> CreateAsync(CreateStudentRequest request);
}
