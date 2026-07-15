using Microsoft.EntityFrameworkCore;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.ClassRegistrations;
using Student_Management_System.Models;
using Student_Management_System.Models.Enum;
using Student_Management_System.Repositories.Interfaces;

namespace Student_Management_System.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly AppDbContext _context;

    public EnrollmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(long studentId, long classroomId)
    {
        return _context.Enrollments.AnyAsync(enrollment =>
            !enrollment.IsDeleted &&
            enrollment.StudentId == studentId &&
            enrollment.ClassroomId == classroomId);
    }

    public Task<Enrollment?> GetByStudentAndClassroomAsync(long studentId, long classroomId)
    {
        return _context.Enrollments.FirstOrDefaultAsync(enrollment =>
            !enrollment.IsDeleted &&
            enrollment.StudentId == studentId &&
            enrollment.ClassroomId == classroomId);
    }

    public Task<Enrollment?> GetActiveByIdAsync(long id)
    {
        return _context.Enrollments.FirstOrDefaultAsync(enrollment => !enrollment.IsDeleted && enrollment.Id == id);
    }

    public async Task<PagedResult<ClassRegistrationItemResponse>> GetPagedRegistrationsAsync(
        EnrollmentStatus? status,
        PaginationQuery pagination)
    {
        var query = _context.Enrollments
            .AsNoTracking()
            .Where(enrollment => !enrollment.IsDeleted);

        if (status is not null)
        {
            query = query.Where(enrollment => enrollment.Status == status);
        }

        var projected = query
            .OrderByDescending(enrollment => enrollment.CreatedAt)
            .Select(enrollment => new ClassRegistrationItemResponse(
                enrollment.Id,
                enrollment.StudentId,
                enrollment.ClassroomId,
                enrollment.Student.Fullname,
                enrollment.Student.Parent == null ? "" : enrollment.Student.Parent.Fullname,
                enrollment.Student.Parent == null ? "" : enrollment.Student.Parent.Phone,
                enrollment.Classroom.Name,
                enrollment.EnrollDate,
                enrollment.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                enrollment.Status.ToString()));

        var total = await projected.CountAsync();
        var items = await projected.Skip(pagination.Skip).Take(pagination.PageSize).ToListAsync();

        return new PagedResult<ClassRegistrationItemResponse>(items, pagination.Page, pagination.PageSize, total);
    }

    public void Add(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
