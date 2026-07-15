using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.ClassRegistrations;
using Student_Management_System.Models.Enum;

namespace Student_Management_System.Services.Interfaces;

public interface IClassRegistrationService
{
    Task<PagedResult<ClassRegistrationItemResponse>> GetPagedAsync(EnrollmentStatus? status, PaginationQuery pagination);
    Task<ClassRegistrationResponse?> RegisterAsync(ClassRegistrationRequest request);
    Task<bool> ApproveAsync(long id);
    Task<bool> RejectAsync(long id);
}
