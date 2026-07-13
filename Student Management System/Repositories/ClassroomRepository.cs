using Microsoft.EntityFrameworkCore;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Classrooms;
using Student_Management_System.Models;
using Student_Management_System.Repositories.Interfaces;

namespace Student_Management_System.Repositories;

public class ClassroomRepository : IClassroomRepository
{
    private readonly AppDbContext _context;

    public ClassroomRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ClassroomResponse>> GetPagedAsync(PaginationQuery pagination)
    {
        var query = _context.Classrooms
            .AsNoTracking()
            .Where(classroom => !classroom.IsDeleted)
            .OrderBy(classroom => classroom.Name)
            .Select(classroom => new ClassroomResponse(
                classroom.Id,
                classroom.Name,
                classroom.TuitionFee,
                classroom.TeacherId,
                classroom.Teacher == null ? null : classroom.Teacher.Fullname,
                classroom.Enrollments.Count(enrollment => !enrollment.IsDeleted)));

        var total = await query.CountAsync();
        var items = await query.Skip(pagination.Skip).Take(pagination.PageSize).ToListAsync();

        return new PagedResult<ClassroomResponse>(items, pagination.Page, pagination.PageSize, total);
    }

    public Task<Classroom?> GetActiveByIdAsync(long id)
    {
        return _context.Classrooms.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
    }

    public void Add(Classroom classroom)
    {
        _context.Classrooms.Add(classroom);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
