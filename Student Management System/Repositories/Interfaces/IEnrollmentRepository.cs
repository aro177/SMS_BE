using Student_Management_System.Models;
using Student_Management_System.Models.Enum;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.ClassRegistrations;

namespace Student_Management_System.Repositories.Interfaces;

public interface IEnrollmentRepository
{
    Task<bool> ExistsAsync(long studentId, long classroomId);
    Task<Enrollment?> GetByStudentAndClassroomAsync(long studentId, long classroomId);
    Task<Enrollment?> GetActiveByIdAsync(long id);
    Task<PagedResult<ClassRegistrationItemResponse>> GetPagedRegistrationsAsync(EnrollmentStatus? status, PaginationQuery pagination);
    void Add(Enrollment enrollment);
    Task SaveChangesAsync();
}
