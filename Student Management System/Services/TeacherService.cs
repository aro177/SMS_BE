using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Teachers;
using Student_Management_System.Models;
using Student_Management_System.Repositories.Interfaces;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Services;

public class TeacherService : ITeacherService
{
    private readonly ITeacherRepository _teachers;

    public TeacherService(ITeacherRepository teachers)
    {
        _teachers = teachers;
    }

    public Task<PagedResult<TeacherResponse>> GetPagedAsync(PaginationQuery pagination)
    {
        return _teachers.GetPagedAsync(pagination);
    }

    public async Task<TeacherResponse> CreateAsync(CreateTeacherRequest request)
    {
        var teacher = new Teacher
        {
            Fullname = request.Fullname.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _teachers.Add(teacher);
        await _teachers.SaveChangesAsync();

        return new TeacherResponse(teacher.Id, teacher.Fullname, teacher.Phone, 0);
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
