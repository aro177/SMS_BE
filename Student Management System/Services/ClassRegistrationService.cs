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

        var exists = await _enrollments.ExistsAsync(student.Id, request.ClassId);
        if (!exists)
        {
            _enrollments.Add(new Enrollment
            {
                StudentId = student.Id,
                ClassroomId = request.ClassId,
                EnrollDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Status = EnrollmentStatus.ACTIVE,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await _students.SaveChangesAsync();
        }

        return new ClassRegistrationResponse(student.Fullname, classroom.Name, EnrollmentStatus.ACTIVE.ToString());
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
