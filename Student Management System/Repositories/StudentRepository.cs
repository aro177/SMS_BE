using Microsoft.EntityFrameworkCore;
using Student_Management_System.Common.Pagination;
using Student_Management_System.Dtos.Students;
using Student_Management_System.Models;
using Student_Management_System.Models.Enum;
using Student_Management_System.Repositories.Interfaces;

namespace Student_Management_System.Repositories;

public class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _context;

    public StudentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<StudentResponse>> GetPagedAsync(PaginationQuery pagination)
    {
        var query = _context.Students
            .AsNoTracking()
            .Where(student => !student.IsDeleted)
            .OrderBy(student => student.Fullname)
            .Select(student => new StudentResponse(
                student.Id,
                student.Fullname,
                student.Dob,
                student.Height,
                student.Weight,
                student.ParentId,
                student.Parent == null ? null : student.Parent.Fullname,
                student.Parent == null ? null : student.Parent.Phone,
                student.Enrollments
                    .Where(enrollment => !enrollment.IsDeleted)
                    .Select(enrollment => enrollment.Classroom.Name)
                    .FirstOrDefault()));

        var total = await query.CountAsync();
        var items = await query.Skip(pagination.Skip).Take(pagination.PageSize).ToListAsync();

        return new PagedResult<StudentResponse>(items, pagination.Page, pagination.PageSize, total);
    }

    public Task<Student?> FindByParentPhoneNameAndDobAsync(string parentPhone, string childName, DateOnly childDob)
    {
        return _context.Students
            .Include(student => student.Parent)
            .FirstOrDefaultAsync(student =>
                !student.IsDeleted &&
                student.Parent != null &&
                student.Parent.Phone == parentPhone &&
                student.Fullname == childName &&
                student.Dob == childDob);
    }

    public async Task<IReadOnlyList<ChildSearchResponse>> SearchChildrenAsync(string parentPhone, DateOnly childDob)
    {
        var students = await _context.Students
            .AsNoTracking()
            .Where(student =>
                !student.IsDeleted &&
                student.Dob == childDob &&
                student.Parent != null &&
                student.Parent.Phone == parentPhone)
            .Select(student => new
            {
                student.Fullname,
                student.Dob,
                ParentPhone = student.Parent == null ? "" : student.Parent.Phone,
                CurrentClass = student.Enrollments
                    .Where(enrollment => !enrollment.IsDeleted)
                    .Select(enrollment => enrollment.Classroom.Name)
                    .FirstOrDefault(),
                TotalAttendances = student.Attendances.Count(attendance => !attendance.IsDeleted),
                PresentAttendances = student.Attendances.Count(attendance => !attendance.IsDeleted && attendance.Status == AttendanceStatus.PRESENT),
                LatestNote = student.Attendances
                    .Where(attendance => !attendance.IsDeleted && attendance.Note != null)
                    .OrderByDescending(attendance => attendance.CreatedAt)
                    .Select(attendance => attendance.Note)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return students
            .Select(student => new ChildSearchResponse(
                student.Fullname,
                student.Dob == null ? "" : student.Dob.Value.ToString("yyyy-MM-dd"),
                student.ParentPhone,
                student.CurrentClass ?? "Chưa có lớp",
                BuildAttendanceRate(student.TotalAttendances, student.PresentAttendances),
                student.LatestNote ?? "Chưa có ghi chú mới"))
            .ToList();
    }

    public void Add(Student student)
    {
        _context.Students.Add(student);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    private static string BuildAttendanceRate(int total, int present)
    {
        if (total == 0)
        {
            return "Chưa có dữ liệu";
        }

        return $"{Math.Round((decimal)present / total * 100)}%";
    }
}
