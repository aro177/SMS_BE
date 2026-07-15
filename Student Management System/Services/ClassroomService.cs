using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Classrooms;
using Student_Management_System.Models;
using Student_Management_System.Repositories.Interfaces;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Services;

public class ClassroomService : IClassroomService
{
    private readonly IClassroomRepository _classrooms;

    public ClassroomService(IClassroomRepository classrooms)
    {
        _classrooms = classrooms;
    }

    public Task<PagedResult<ClassroomResponse>> GetPagedAsync(PaginationQuery pagination)
    {
        return _classrooms.GetPagedAsync(pagination);
    }

    public async Task<ClassroomResponse> CreateAsync(CreateClassroomRequest request)
    {
        var classroom = new Classroom
        {
            Name = request.Name.Trim(),
            TeacherId = request.TeacherId,
            TuitionFee = request.TuitionFee,
            AgeGroup = string.IsNullOrWhiteSpace(request.AgeGroup) ? null : request.AgeGroup.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Capacity = request.Capacity <= 0 ? 20 : request.Capacity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _classrooms.Add(classroom);
        await _classrooms.SaveChangesAsync();

        return new ClassroomResponse(
            classroom.Id,
            classroom.Name,
            classroom.TuitionFee,
            classroom.TeacherId,
            null,
            0,
            classroom.AgeGroup,
            classroom.Description,
            classroom.Capacity);
    }

    public async Task<bool> UpdateAsync(long id, UpdateClassroomRequest request)
    {
        var classroom = await _classrooms.GetActiveByIdAsync(id);
        if (classroom is null)
        {
            return false;
        }

        classroom.Name = request.Name.Trim();
        classroom.TeacherId = request.TeacherId;
        classroom.TuitionFee = request.TuitionFee;
        classroom.AgeGroup = string.IsNullOrWhiteSpace(request.AgeGroup) ? null : request.AgeGroup.Trim();
        classroom.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        classroom.Capacity = request.Capacity <= 0 ? 20 : request.Capacity;
        classroom.UpdatedAt = DateTime.UtcNow;

        await _classrooms.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var classroom = await _classrooms.GetActiveByIdAsync(id);
        if (classroom is null)
        {
            return false;
        }

        classroom.IsDeleted = true;
        classroom.UpdatedAt = DateTime.UtcNow;

        await _classrooms.SaveChangesAsync();
        return true;
    }
}
