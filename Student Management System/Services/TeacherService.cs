using Student_Management_System.Common.Pagination;
using Student_Management_System.Configs.HttpContext;
using Student_Management_System.Dtos.Teachers;
using Student_Management_System.Models;
using Student_Management_System.Repositories.Interfaces;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Services;

public class TeacherService : ITeacherService
{
    private readonly ICurrentUserService _currentUser;
    private readonly ITeacherRepository _teachers;

    public TeacherService(ICurrentUserService currentUser, ITeacherRepository teachers)
    {
        _currentUser = currentUser;
        _teachers = teachers;
    }

    public Task<PagedResult<TeacherResponse>> GetPagedAsync(PaginationQuery pagination)
    {
        return _teachers.GetPagedAsync(pagination);
    }

    public async Task<TeacherResponse?> GetCurrentTeacherAsync()
    {
        var userId = _currentUser.User?.UserId;
        if (userId is null)
        {
            return null;
        }

        var teacher = await _teachers.GetActiveByAuthUserIdAsync(userId.Value);
        return teacher is null
            ? null
            : new TeacherResponse(
                teacher.Id,
                teacher.Fullname,
                teacher.Phone,
                teacher.Classrooms.Count(classroom => !classroom.IsDeleted),
                teacher.AuthUserId);
    }

    public async Task<TeacherResponse> CreateAsync(CreateTeacherRequest request)
    {
        var teacher = new Teacher
        {
            Fullname = request.Fullname.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            AuthUserId = request.AuthUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _teachers.Add(teacher);
        await _teachers.SaveChangesAsync();

        return new TeacherResponse(teacher.Id, teacher.Fullname, teacher.Phone, 0, teacher.AuthUserId);
    }

    public async Task<bool> UpdateAsync(long id, UpdateTeacherRequest request)
    {
        var teacher = await _teachers.GetActiveByIdAsync(id);
        if (teacher is null)
        {
            return false;
        }

        teacher.Fullname = request.Fullname.Trim();
        teacher.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        teacher.AuthUserId = request.AuthUserId;
        teacher.UpdatedAt = DateTime.UtcNow;

        await _teachers.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var teacher = await _teachers.GetActiveByIdAsync(id);
        if (teacher is null)
        {
            return false;
        }

        teacher.IsDeleted = true;
        teacher.UpdatedAt = DateTime.UtcNow;

        await _teachers.SaveChangesAsync();
        return true;
    }
}
