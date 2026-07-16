namespace Student_Management_System.Dtos.Teachers;

public record TeacherResponse(long Id, string Fullname, string? Phone, int ClassesCount, Guid? AuthUserId = null);

public record CreateTeacherRequest(string Fullname, string? Phone, Guid? AuthUserId = null);

public record UpdateTeacherRequest(string Fullname, string? Phone, Guid? AuthUserId = null);
