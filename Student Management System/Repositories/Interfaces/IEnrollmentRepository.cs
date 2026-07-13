using Student_Management_System.Models;

namespace Student_Management_System.Repositories.Interfaces;

public interface IEnrollmentRepository
{
    Task<bool> ExistsAsync(long studentId, long classroomId);
    void Add(Enrollment enrollment);
}
