using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.ClassRegistrations;
using Student_Management_System.Models;
using Student_Management_System.Models.Enum;
using Student_Management_System.Repositories.Interfaces;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Services;

public class ClassRegistrationService : IClassRegistrationService
{
    private readonly IClassroomRepository _classrooms;
    private readonly IEnrollmentRepository _enrollments;
    private readonly IParentRepository _parents;
    private readonly IStudentRepository _students;

    public ClassRegistrationService(
        IClassroomRepository classrooms,
        IEnrollmentRepository enrollments,
        IParentRepository parents,
        IStudentRepository students)
    {
        _classrooms = classrooms;
        _enrollments = enrollments;
        _parents = parents;
        _students = students;
    }

    public Task<PagedResult<ClassRegistrationItemResponse>> GetPagedAsync(EnrollmentStatus? status, PaginationQuery pagination)
    {
        return _enrollments.GetPagedRegistrationsAsync(status, pagination);
    }

    public async Task<ClassRegistrationResponse?> RegisterAsync(ClassRegistrationRequest request)
    {
        var classroom = await _classrooms.GetActiveByIdAsync(request.ClassId);
        if (classroom is null)
        {
            return null;
        }

        var parent = await FindOrCreateParent(request.ParentName, request.ParentPhone);
        var student = await _students.FindByParentPhoneNameAndDobAsync(
            parent.Phone,
            request.ChildName.Trim(),
            request.ChildDob);

        if (student is null)
        {
            student = new Student
            {
                Fullname = request.ChildName.Trim(),
                Dob = request.ChildDob,
                Parent = parent,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _students.Add(student);
            await _students.SaveChangesAsync();
        }

        var enrollment = await _enrollments.GetByStudentAndClassroomAsync(student.Id, request.ClassId);
        if (enrollment is null)
        {
            enrollment = new Enrollment
            {
                StudentId = student.Id,
                ClassroomId = request.ClassId,
                EnrollDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Status = EnrollmentStatus.PENDING,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _enrollments.Add(enrollment);
        }
        else
        {
            enrollment.Status = EnrollmentStatus.PENDING;
            enrollment.UpdatedAt = DateTime.UtcNow;
        }

        await _enrollments.SaveChangesAsync();

        return new ClassRegistrationResponse(student.Fullname, classroom.Name, EnrollmentStatus.PENDING.ToString());
    }

    public async Task<bool> ApproveAsync(long id)
    {
        return await UpdateStatusAsync(id, EnrollmentStatus.ACTIVE);
    }

    public async Task<bool> RejectAsync(long id)
    {
        return await UpdateStatusAsync(id, EnrollmentStatus.DROPPED);
    }

    private async Task<bool> UpdateStatusAsync(long id, EnrollmentStatus status)
    {
        var enrollment = await _enrollments.GetActiveByIdAsync(id);
        if (enrollment is null)
        {
            return false;
        }

        enrollment.Status = status;
        enrollment.UpdatedAt = DateTime.UtcNow;

        await _enrollments.SaveChangesAsync();
        return true;
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
