using Microsoft.EntityFrameworkCore;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Teachers;
using Student_Management_System.Models;
using Student_Management_System.Repositories.Interfaces;

namespace Student_Management_System.Repositories;

public class TeacherRepository : ITeacherRepository
{
    private readonly AppDbContext _context;

    public TeacherRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TeacherResponse>> GetPagedAsync(PaginationQuery pagination)
    {
        var query = _context.Teachers
            .AsNoTracking()
            .Where(teacher => !teacher.IsDeleted)
            .OrderBy(teacher => teacher.Fullname)
            .Select(teacher => new TeacherResponse(
                teacher.Id,
                teacher.Fullname,
                teacher.Phone,
                teacher.Classrooms.Count(classroom => !classroom.IsDeleted),
                teacher.AuthUserId));

        var total = await query.CountAsync();
        var items = await query.Skip(pagination.Skip).Take(pagination.PageSize).ToListAsync();

        return new PagedResult<TeacherResponse>(items, pagination.Page, pagination.PageSize, total);
    }

    public Task<Teacher?> GetActiveByIdAsync(long id)
    {
        return _context.Teachers.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted);
    }

    public Task<Teacher?> GetActiveByAuthUserIdAsync(Guid authUserId)
    {
        return _context.Teachers.FirstOrDefaultAsync(item => item.AuthUserId == authUserId && !item.IsDeleted);
    }

    public void Add(Teacher teacher)
    {
        _context.Teachers.Add(teacher);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
