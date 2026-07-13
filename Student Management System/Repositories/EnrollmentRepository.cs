using Microsoft.EntityFrameworkCore;
using Student_Management_System.Models;
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

    public void Add(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
    }
}
