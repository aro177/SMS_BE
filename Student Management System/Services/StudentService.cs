using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Students;
using Student_Management_System.Models;
using Student_Management_System.Repositories.Interfaces;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Services;

public class StudentService : IStudentService
{
    private readonly IParentRepository _parents;
    private readonly IStudentRepository _students;

    public StudentService(IParentRepository parents, IStudentRepository students)
    {
        _parents = parents;
        _students = students;
    }

    public Task<PagedResult<StudentResponse>> GetPagedAsync(PaginationQuery pagination)
    {
        return _students.GetPagedAsync(pagination);
    }

    public Task<IReadOnlyList<ChildSearchResponse>> SearchChildrenAsync(string parentPhone, DateOnly childDob)
    {
        return _students.SearchChildrenAsync(parentPhone.Trim(), childDob);
    }

    public async Task<StudentResponse> CreateAsync(CreateStudentRequest request)
    {
        var parent = await FindOrCreateParent(request.ParentName, request.ParentPhone);
        var student = new Student
        {
            Fullname = request.Fullname.Trim(),
            Dob = request.Dob,
            Height = request.Height,
            Weight = request.Weight,
            Parent = parent,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _students.Add(student);
        await _students.SaveChangesAsync();

        return new StudentResponse(
            student.Id,
            student.Fullname,
            student.Dob,
            student.Height,
            student.Weight,
            student.ParentId,
            parent.Fullname,
            parent.Phone,
            null);
    }

    private async Task<Parent> FindOrCreateParent(string parentName, string parentPhone)
    {
        var phone = parentPhone.Trim();
        var parent = await _parents.FindByPhoneAsync(phone);
        if (parent is not null)
        {
            return parent;
        }

        parent = new Parent
        {
            Fullname = parentName.Trim(),
            Phone = phone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _parents.Add(parent);

        return parent;
    }
}
