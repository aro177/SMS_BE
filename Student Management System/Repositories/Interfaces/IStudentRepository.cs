using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Students;
using Student_Management_System.Models;

namespace Student_Management_System.Repositories.Interfaces;

public interface IStudentRepository
{
    Task<PagedResult<StudentResponse>> GetPagedAsync(PaginationQuery pagination);
    Task<IReadOnlyList<ChildSearchResponse>> SearchChildrenAsync(string parentPhone, DateOnly childDob);
    Task<Student?> FindByParentPhoneNameAndDobAsync(string parentPhone, string childName, DateOnly childDob);
    void Add(Student student);
    Task SaveChangesAsync();
}
